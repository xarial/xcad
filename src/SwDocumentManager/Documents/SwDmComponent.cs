//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Services;
using Xarial.XCad.SwDocumentManager.Services;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmComponent : IXComponent, ISwDmSelObject
    {
        string CachedPath { get; }
        ISwDMComponent Component { get; }
        new ISwDmDocument3D ReferencedDocument { get; }
        new ISwDmConfiguration ReferencedConfiguration { get; }
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class SwDmComponent : SwDmSelObject, ISwDmComponent
    {
        #region Not Supported

        public IXFeatureRepository Features => throw new NotSupportedException();
        public IXBodyRepository Bodies => throw new NotSupportedException();
        TSelObject IXObjectContainer.ConvertObject<TSelObject>(TSelObject obj) => throw new NotSupportedException();
        public Color? Color { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public IXDimensionRepository Dimensions => throw new NotSupportedException();

        #endregion

        IXDocument3D IXComponent.ReferencedDocument => ReferencedDocument;
        IXConfiguration IXComponent.ReferencedConfiguration => ReferencedConfiguration;

        public ISwDMComponent Component { get; }

        public override SelectType_e Type => SelectType_e.Components;

        internal SwDmAssembly ParentAssembly { get; }

        private IFilePathResolver m_FilePathResolver;

        private readonly Lazy<string> m_PathLazy;
        private readonly Lazy<IXComponentRepository> m_ChildrenLazy;
        private readonly Lazy<ISwDmConfiguration> m_ReferencedConfigurationLazy;
        
        internal SwDmComponent(SwDmAssembly parentAssm, ISwDMComponent comp) : base(comp)
        {
            Component = comp;
            ParentAssembly = parentAssm;
            m_FilePathResolver = new SwDmFilePathResolver();

            m_PathLazy = new Lazy<string>(() => 
            {
                var rootDir = System.IO.Path.GetDirectoryName(ParentAssembly.Path);

                return m_FilePathResolver.ResolvePath(rootDir, CachedPath);
            });

            m_ChildrenLazy = new Lazy<IXComponentRepository>(() => 
            {
                if (!Component.IsSuppressed() && ReferencedDocument is SwDmAssembly)
                {
                    var refConf = ReferencedConfiguration;

                    if (!refConf.IsCommitted)
                    {
                        refConf.Commit();
                    }

                    return new SwDmSubComponentCollection(this, (SwDmAssembly)ReferencedDocument, refConf);
                }
                else
                {
                    return new EmptyComponentCollection();
                }
            });

            m_ReferencedConfigurationLazy = new Lazy<ISwDmConfiguration>(() => new SwDmComponentConfiguration(this));
        }

        public string Name => ((ISwDMComponent7)Component).Name2;

        public string FullName
        {
            get
            {
                if (Parent != null)
                {
                    return Parent.FullName + "/" + Name;
                }
                else 
                {
                    return Name;
                }   
            }
        }

        public string CachedPath => ((ISwDMComponent6)Component).PathName;

        public string Path => m_PathLazy.Value;

        public ISwDmConfiguration ReferencedConfiguration => m_ReferencedConfigurationLazy.Value;

        public ComponentState_e State
        {
            get
            {
                var state = ComponentState_e.Default;

                if (Component.IsSuppressed())
                {
                    state |= ComponentState_e.Suppressed;
                }

                if (Component.IsHidden())
                {
                    if (!state.HasFlag(ComponentState_e.Suppressed))//Document Manager reports suppressed as hidden as well
                    {
                        state |= ComponentState_e.Hidden;
                    }
                }

                if (((ISwDMComponent5)Component).ExcludeFromBOM == (int)swDmExcludeFromBOMResult.swDmExcludeFromBOM_TRUE)
                {
                    state |= ComponentState_e.ExcludedFromBom;
                }

                if (((ISwDMComponent5)Component).IsEnvelope())
                {
                    state |= ComponentState_e.Envelope;
                }

                if (((ISwDMComponent3)Component).IsVirtual)
                {
                    state |= ComponentState_e.Embedded;
                }

                return state;
            }
            set 
            {
                if (((ISwDMComponent5)Component).ExcludeFromBOM == (int)swDmExcludeFromBOMResult.swDmExcludeFromBOM_TRUE
                    && !value.HasFlag(ComponentState_e.ExcludedFromBom))
                {
                    ((ISwDMComponent5)Component).ExcludeFromBOM = (int)swDmExcludeFromBOMResult.swDmExcludeFromBOM_TRUE;
                }
            }
        }

        public TransformMatrix Transformation
        {
            get 
            {
                var data = (double[])Component.Transform;

                var scale = data[15];

                var transform = new TransformMatrix(
                    data[0] * scale, data[1] * scale, data[2] * scale, 0,
                    data[4] * scale, data[5] * scale, data[6] * scale, 0,
                    data[8] * scale, data[9] * scale, data[10] * scale, 0,
                    data[12], data[13], data[14], 1);

                if (Parent != null) 
                {
                    transform = transform.Multiply(Parent.Transformation);
                }

                return transform;
            }
            set => throw new NotSupportedException("Transform of the component cannot be modified"); 
        }

        private ISwDmDocument3D m_CachedDocument;

        public ISwDmDocument3D ReferencedDocument
        {
            get
            {
                if (m_CachedDocument == null)
                {
                    var isReadOnly = ParentAssembly.State.HasFlag(DocumentState_e.ReadOnly);

                    var docsColl = (SwDmDocumentCollection)ParentAssembly.SwDmApp.Documents;

                    try
                    {
                        ISwDmDocument3D doc;

                        var isVirtual = ((ISwDMComponent3)Component).IsVirtual;

                        //NOTE: Do not use ISwDMComponent4::GetDocument2 to get the document as it will firstly load the file from the cached path which may result in th wrong file loaded if assembly is copied
                        if (isVirtual)
                        {
                            var searchOpts = ParentAssembly.SwDmApp.SwDocMgr.GetSearchOptionObject();

                            searchOpts.AddSearchPath(System.IO.Path.GetDirectoryName(ParentAssembly.Path));

                            searchOpts.SearchFilters = (int)(SwDmSearchFilters.SwDmSearchForAssembly
                                | SwDmSearchFilters.SwDmSearchForPart
                                | SwDmSearchFilters.SwDmSearchRootAssemblyFolder
                                | SwDmSearchFilters.SwDmSearchSubfolders
                                | SwDmSearchFilters.SwDmSearchExternalReference
                                | SwDmSearchFilters.SwDmSearchInContextReference);

                            var dmDoc = ((ISwDMComponent4)Component).GetDocument2(isReadOnly, searchOpts, out SwDmDocumentOpenError err);

                            if (dmDoc == null) 
                            {
                                throw new NullReferenceException("Failed to load virtual component document");
                            }

                            doc = (ISwDmDocument3D)ParentAssembly.SwDmApp.Documents
                                    .FirstOrDefault(d => ((ISwDmDocument)d).Document == d);

                            if (doc == null)
                            {
                                var docType = ((ISwDMComponent2)Component).DocumentType;
                                
                                switch (docType)
                                {
                                    case SwDmDocumentType.swDmDocumentPart:
                                        doc = new SwDmVirtualPart(ParentAssembly.SwDmApp, dmDoc, ParentAssembly, true,
                                            docsColl.OnDocumentCreated,
                                            docsColl.OnDocumentClosed, isReadOnly);
                                        break;
                                    case SwDmDocumentType.swDmDocumentAssembly:
                                        doc = new SwDmVirtualAssembly(ParentAssembly.SwDmApp, dmDoc, ParentAssembly, true,
                                            docsColl.OnDocumentCreated,
                                            docsColl.OnDocumentClosed, isReadOnly);
                                        break;
                                    default:
                                        throw new NotSupportedException($"Document type '{docType}' of the component is not supported");
                                }

                                docsColl.OnDocumentCreated(doc);
                            }
                        }
                        else 
                        {
                            if (ParentAssembly.SwDmApp.Documents.TryGet(Path, out ISwDmDocument curDoc))
                            {
                                doc = (ISwDmDocument3D)curDoc;
                            }
                            else
                            {
                                doc = (ISwDmDocument3D)ParentAssembly.SwDmApp.Documents.PreCreateFromPath(Path);
                                doc.State = isReadOnly ? DocumentState_e.ReadOnly : DocumentState_e.Default;
                            }

                            if (!doc.IsCommitted) 
                            {
                                doc.Commit();
                            }
                        }

                        m_CachedDocument = doc;
                    }
                    catch 
                    {
                        var unknownDoc = new SwDmUnknownDocument(ParentAssembly.SwDmApp, null,
                                false,
                                docsColl.OnDocumentCreated,
                                docsColl.OnDocumentClosed, isReadOnly);

                        unknownDoc.Path = CachedPath;

                        m_CachedDocument = (ISwDmDocument3D)unknownDoc.GetSpecific();
                    }

                    return m_CachedDocument;
                }

                return m_CachedDocument;
            }
        }

        public IXComponentRepository Children => m_ChildrenLazy.Value;

        internal SwDmComponent Parent { get; set; }
    }

    internal class SwDmComponentConfiguration : SwDmConfiguration
    {
        private readonly ISwDmComponent m_Comp;

        internal SwDmComponentConfiguration(ISwDmComponent comp) : base(null, null)
        {
            m_Comp = comp;
        }
        
        public override string Name 
        {
            get => m_Comp.Component.ConfigurationName;
            set => throw new NotSupportedException();
        }

        internal protected override SwDmDocument3D Document => (SwDmDocument3D)m_Comp.ReferencedDocument;

        public override ISwDMConfiguration Configuration => Document.Configurations[Name].Configuration;

        public override object Dispatch => Configuration;

        public override bool IsCommitted => Document.IsCommitted;
    }

    internal class SwDmSubComponentCollection : SwDmComponentCollection
    {
        private readonly SwDmComponent m_ParentComp;

        internal SwDmSubComponentCollection(SwDmComponent parentComp,
            SwDmAssembly rootAssm, ISwDmConfiguration conf) 
            : base(rootAssm, conf)
        {
            m_ParentComp = parentComp;
        }

        protected override SwDmComponent CreateComponentInstance(ISwDMComponent dmComp)
        {
            var comp =  base.CreateComponentInstance(dmComp);
            comp.Parent = m_ParentComp;
            return comp;
        }
    }

    internal class EmptyComponentCollection : IXComponentRepository
    {
        #region Not Supported
        
        public void AddRange(IEnumerable<IXComponent> ents)
            => throw new NotSupportedException();

        public void RemoveRange(IEnumerable<IXComponent> ents)
            => throw new NotSupportedException();

        #endregion

        public IXComponent this[string name] => throw new Exception("No components");

        public int TotalCount => 0;

        public int Count => 0;

        public IEnumerator<IXComponent> GetEnumerator()
        {
            yield break; 
        }

        public bool TryGet(string name, out IXComponent ent)
            => throw new Exception("No components");

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}

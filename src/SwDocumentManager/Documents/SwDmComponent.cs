//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Microsoft.VisualBasic;
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
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Services;
using Xarial.XCad.SwDocumentManager.Features;
using Xarial.XCad.SwDocumentManager.Services;
using Xarial.XCad.Toolkit.Utils;
using static Xarial.XCad.SwDocumentManager.Documents.SwDmDocument;

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
    internal abstract class SwDmComponent : SwDmSelObject, ISwDmComponent
    {
        #region Not Supported
        public IXFeatureRepository Features => throw new NotSupportedException();
        public IXBodyRepository Bodies => throw new NotSupportedException();
        TSelObject IXObjectContainer.ConvertObject<TSelObject>(TSelObject obj) => throw new NotSupportedException();
        public Color? Color { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public IXDimensionRepository Dimensions => throw new NotSupportedException();
        public IEditor<IXComponent> Edit() => throw new NotSupportedException();

        #endregion

        IXDocument3D IXComponent.ReferencedDocument { get => ReferencedDocument; set => ReferencedDocument = (ISwDmDocument3D)value; }
        IXConfiguration IXComponent.ReferencedConfiguration { get => ReferencedConfiguration; set => ReferencedConfiguration = (ISwDmConfiguration)value; }
        IXComponent IXComponent.Parent => Parent;

        public ISwDMComponent Component { get; }

        internal SwDmAssembly ParentAssembly { get; }

        private IFilePathResolver m_FilePathResolver;

        private readonly Lazy<string> m_PathLazy;
        private readonly Lazy<IXComponentRepository> m_ChildrenLazy;
                
        internal SwDmComponent(SwDmAssembly parentAssm, ISwDMComponent comp) : base(comp, parentAssm.OwnerApplication, parentAssm)
        {
            Component = comp;
            ParentAssembly = parentAssm;
            m_FilePathResolver = new SwDmFilePathResolver();

            m_PathLazy = new Lazy<string>(() => 
            {
                var rootDir = Path.GetDirectoryName(ParentAssembly.Path);

                var cachedPath = CachedPath;

                var changedPath = ParentAssembly.ChangedReferences.EnumerateByFileName(cachedPath).LastOrDefault();

                if (!string.IsNullOrEmpty(changedPath))
                {
                    cachedPath = changedPath;
                }

                return m_FilePathResolver.ResolvePath(rootDir, cachedPath);
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
        }

        public string Name
        {
            get => ((ISwDMComponent7)Component).Name2;
            set => throw new NotSupportedException();
        }

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

        private string GetPath() => m_PathLazy.Value;

        public ISwDmConfiguration ReferencedConfiguration 
        {
            get => GetReferencedConfiguration(Component.ConfigurationName);
            set => throw new NotSupportedException();
        }

        protected internal abstract ISwDmConfiguration GetReferencedConfiguration(string confName);
        protected abstract ISwDmDocument3D GetReferencedDocument();

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
                var isExcludedFromBom = ((ISwDMComponent5)Component).ExcludeFromBOM == (int)swDmExcludeFromBOMResult.swDmExcludeFromBOM_TRUE;

                if (isExcludedFromBom && !value.HasFlag(ComponentState_e.ExcludedFromBom))
                {
                    ((ISwDMComponent5)Component).ExcludeFromBOM = (int)swDmExcludeFromBOMResult.swDmExcludeFromBOM_FALSE;
                }
                else if (!isExcludedFromBom && value.HasFlag(ComponentState_e.ExcludedFromBom))
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

        public string Reference 
        {
            get 
            {
                if (ParentAssembly.IsVersionNewerOrEqual(SwDmVersion_e.Sw2018))
                {
                    return ((ISwDMComponent10)Component).ComponentReference;
                }
                else 
                {
                    throw new NotSupportedException("This property is only supported from SOLIDWORKS 2018 or newer");
                }
            }
            set => throw new NotSupportedException(); 
        }

        private ISwDmDocument3D m_CachedDocument;
        
        public IXComponentRepository Children => m_ChildrenLazy.Value;

        protected TDocument GetSpecificReferencedDocument<TDocument>()
            where TDocument : ISwDmDocument3D
        {
            if (m_CachedDocument == null)
            {
                var isReadOnly = ParentAssembly.State.HasFlag(DocumentState_e.ReadOnly);

                var docsColl = (SwDmDocumentCollection)ParentAssembly.OwnerApplication.Documents;

                try
                {
                    ISwDmDocument3D doc;

                    var isVirtual = ((ISwDMComponent3)Component).IsVirtual;

                    //NOTE: Do not use ISwDMComponent4::GetDocument2 to get the document as it will firstly load the file from the cached path which may result in the wrong file loaded if assembly is copied
                    if (isVirtual)
                    {
                        var searchOpts = ParentAssembly.OwnerApplication.SwDocMgr.GetSearchOptionObject();

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

                        doc = (ISwDmDocument3D)ParentAssembly.OwnerApplication.Documents
                                .FirstOrDefault(d => ((ISwDmDocument)d).Document == d);

                        if (doc == null)
                        {
                            var docType = ((ISwDMComponent2)Component).DocumentType;

                            switch (docType)
                            {
                                case SwDmDocumentType.swDmDocumentPart:
                                    doc = new SwDmVirtualPart(ParentAssembly.OwnerApplication, dmDoc, ParentAssembly, true,
                                        docsColl.OnDocumentCreated,
                                        docsColl.OnDocumentClosed, isReadOnly);
                                    break;
                                case SwDmDocumentType.swDmDocumentAssembly:
                                    doc = new SwDmVirtualAssembly(ParentAssembly.OwnerApplication, dmDoc, ParentAssembly, true,
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
                        var path = GetPath();

                        if (ParentAssembly.OwnerApplication.Documents.TryGet(path, out ISwDmDocument curDoc))
                        {
                            doc = (ISwDmDocument3D)curDoc;
                        }
                        else
                        {
                            doc = (ISwDmDocument3D)ParentAssembly.OwnerApplication.Documents.PreCreateFromPath(path);
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
                    var unknownDoc = new SwDmUnknownDocument(ParentAssembly.OwnerApplication, null,
                            false,
                            docsColl.OnDocumentCreated,
                            docsColl.OnDocumentClosed, isReadOnly);

                    unknownDoc.Path = CachedPath;

                    m_CachedDocument = (ISwDmDocument3D)unknownDoc.GetSpecific();
                }

                return (TDocument)m_CachedDocument;
            }

            return (TDocument)m_CachedDocument;
        }

        internal SwDmComponent Parent { get; set; }

        public ISwDmDocument3D ReferencedDocument 
        {
            get => GetReferencedDocument();
            set => throw new NotSupportedException();
        }
    }

    internal class SwDmPartComponent : SwDmComponent, IXPartComponent
    {
        IXPart IXPartComponent.ReferencedDocument { get => (IXPart)base.ReferencedDocument; set => base.ReferencedDocument = (ISwDmDocument3D)value; }
        IXPartConfiguration IXPartComponent.ReferencedConfiguration { get => (IXPartConfiguration)base.ReferencedConfiguration; set => base.ReferencedConfiguration = (ISwDmConfiguration)value; }

        public SwDmPartComponent(SwDmAssembly parentAssm, ISwDMComponent comp) : base(parentAssm, comp)
        {
        }

        protected internal override ISwDmConfiguration GetReferencedConfiguration(string confName) => new SwDmPartComponentConfiguration(this, confName);
        protected override ISwDmDocument3D GetReferencedDocument() => GetSpecificReferencedDocument<ISwDmPart>();
    }

    internal class SwDmAssemblyComponent : SwDmComponent, IXAssemblyComponent
    {
        IXAssembly IXAssemblyComponent.ReferencedDocument { get => (IXAssembly)base.ReferencedDocument; set => base.ReferencedDocument = (ISwDmDocument3D)value; }
        IXAssemblyConfiguration IXAssemblyComponent.ReferencedConfiguration { get => (IXAssemblyConfiguration)base.ReferencedConfiguration; set => base.ReferencedConfiguration = (ISwDmConfiguration)value; }

        public SwDmAssemblyComponent(SwDmAssembly parentAssm, ISwDMComponent comp) : base(parentAssm, comp)
        {
        }

        protected internal override ISwDmConfiguration GetReferencedConfiguration(string confName) => new SwDmAssemblyComponentConfiguration(this, confName);
        protected override ISwDmDocument3D GetReferencedDocument() => GetSpecificReferencedDocument<ISwDmAssembly>();
    }

    internal abstract class SwDmComponentConfiguration : SwDmConfiguration
    {
        #region Not Supported
        public IXMaterial Material { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        #endregion

        protected readonly SwDmComponent m_Comp;

        private readonly string m_ConfName;

        internal SwDmComponentConfiguration(SwDmComponent comp, string confName) : base(null, (SwDmDocument3D)comp.OwnerDocument)
        {
            m_Comp = comp;
            m_ConfName = confName;
        }
        
        public override string Name 
        {
            get => m_ConfName;
            set => throw new NotSupportedException();
        }

        public override IXConfiguration Parent
        {
            get
            {
                var parentConfName = Configuration.GetParentConfigurationName();

                if (!string.IsNullOrEmpty(parentConfName))
                {
                    return m_Comp.GetReferencedConfiguration(parentConfName);
                }
                else 
                {
                    return null;
                }
            }
        }

        internal protected override SwDmDocument3D Document => (SwDmDocument3D)m_Comp.ReferencedDocument;

        public override ISwDMConfiguration Configuration => Document.Configurations[Name].Configuration;

        public override object Dispatch => Configuration;

        public override bool IsCommitted => Document.IsCommitted;
    }

    internal class SwDmPartComponentConfiguration : SwDmComponentConfiguration, ISwDmPartConfiguration
    {
        private readonly Lazy<IXCutListItemRepository> m_CutListsLazy;

        public SwDmPartComponentConfiguration(SwDmComponent comp, string confName) : base(comp, confName)
        {
            m_CutListsLazy = new Lazy<IXCutListItemRepository>(
                () => new SwDmCutListItemCollection(this, (SwDmPart)Document));
        }

        public IXCutListItemRepository CutLists => m_CutListsLazy.Value;
    }

    internal class SwDmAssemblyComponentConfiguration : SwDmComponentConfiguration, ISwDmAssemblyConfiguration
    {
        public SwDmAssemblyComponentConfiguration(SwDmComponent comp, string confName) : base(comp, confName)
        {
        }

        public IXComponentRepository Components => m_Comp.Children;
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
        public event ComponentInsertedDelegate ComponentInserted { add => throw new NotSupportedException(); remove => throw new NotSupportedException(); }
        public void AddRange(IEnumerable<IXComponent> ents, CancellationToken cancellationToken) => throw new NotSupportedException();
        public void RemoveRange(IEnumerable<IXComponent> ents, CancellationToken cancellationToken) => throw new NotSupportedException();
        public T PreCreate<T>() where T : IXComponent => throw new NotSupportedException();
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

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) => RepositoryHelper.FilterDefault(this, filters, reverseOrder);
    }
}

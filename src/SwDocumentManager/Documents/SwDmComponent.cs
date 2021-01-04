using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Services;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmComponent : IXComponent, ISwDmSelObject
    {
        ISwDMComponent Component { get; }
        new ISwDmDocument3D Document { get; }
        new ISwDmConfiguration ReferencedConfiguration { get; }
    }

    internal class SwDmComponent : SwDmSelObject, ISwDmComponent
    {
        #region Not Supported

        public IXFeatureRepository Features => throw new NotSupportedException();
        public IXBodyRepository Bodies => throw new NotSupportedException();
        TSelObject IXObjectContainer.ConvertObject<TSelObject>(TSelObject obj) => throw new NotSupportedException();

        #endregion

        IXDocument3D IXComponent.Document => Document;
        IXConfiguration IXComponent.ReferencedConfiguration => ReferencedConfiguration;

        public ISwDMComponent Component { get; }

        private SwDmAssembly m_ParentAssm;

        internal SwDmComponent(SwDmAssembly parentAssm, ISwDMComponent comp) : base(comp)
        {
            Component = comp;
            m_ParentAssm = parentAssm;
        }

        public string Name
        {
            get
            {
                var fullName = new StringBuilder(((ISwDMComponent7)Component).Name2);

                var curParent = Parent;

                while (curParent != null) 
                {
                    fullName.Insert(0, curParent.Name + "/");
                    curParent = curParent.Parent;
                }

                return fullName.ToString();
            }
        }

        //TODO: check - this migth be a cached path
        public string Path => ((ISwDMComponent6)Component).PathName;

        public ISwDmConfiguration ReferencedConfiguration => new SwDmComponentConfiguration(this, m_CachedDocument);

        public ComponentState_e State
        {
            get
            {
                var state = ComponentState_e.Default;

                if (Component.IsHidden())
                {
                    state |= ComponentState_e.Hidden;
                }

                if (Component.IsSuppressed())
                {
                    state |= ComponentState_e.Suppressed;
                }

                if (((ISwDMComponent5)Component).ExcludeFromBOM == (int)swDmExcludeFromBOMResult.swDmExcludeFromBOM_TRUE)
                {
                    state |= ComponentState_e.ExcludedFromBom;
                }

                return state;
            }
        }

        private ISwDmDocument3D m_CachedDocument;

        public ISwDmDocument3D Document
        {
            get
            {
                if (m_CachedDocument == null || !m_CachedDocument.IsAlive)
                {
                    var searchOpts = m_ParentAssm.SwDmApp.SwDocMgr.GetSearchOptionObject();
                    searchOpts.SearchFilters = (int)(
                        SwDmSearchFilters.SwDmSearchExternalReference
                        | SwDmSearchFilters.SwDmSearchRootAssemblyFolder
                        | SwDmSearchFilters.SwDmSearchSubfolders
                        | SwDmSearchFilters.SwDmSearchInContextReference);

                    var isReadOnly = m_ParentAssm.State.HasFlag(DocumentState_e.ReadOnly);

                    var doc = ((ISwDMComponent4)Component).GetDocument2(isReadOnly,
                        searchOpts, out SwDmDocumentOpenError err) ;

                    m_CachedDocument = (ISwDmDocument3D)new SwDmUnknownDocument(m_ParentAssm.SwDmApp, doc,
                        true,
                        ((SwDmDocumentCollection)m_ParentAssm.SwDmApp.Documents).OnDocumentCreated,
                        ((SwDmDocumentCollection)m_ParentAssm.SwDmApp.Documents).OnDocumentClosed, isReadOnly).GetSpecific();
                }

                return m_CachedDocument;
            }
        }

        public IXComponentRepository Children
        {
            get
            {
                var refConf = ReferencedConfiguration;
                
                if (!refConf.IsCommitted) 
                {
                    refConf.Commit(default);
                }

                return new SwDmSubComponentCollection(this, m_ParentAssm, refConf);
            }
        }

        public SwDmComponent Parent { get; internal set; }
    }

    internal class SwDmComponentConfiguration : SwDmConfiguration
    {
        private readonly ISwDmComponent m_Comp;

        private readonly ElementCreator<ISwDMConfiguration> m_Creator;

        internal SwDmComponentConfiguration(ISwDmComponent comp, ISwDmDocument3D doc) : base(null, null)
        {
            m_Comp = comp;

            var conf = doc != null ? CreateConfiguration(default) : null;
            m_Creator = new ElementCreator<ISwDMConfiguration>(CreateConfiguration, conf, conf != null);
        }

        private ISwDMConfiguration CreateConfiguration(CancellationToken cancellationToken)
            => (ISwDMConfiguration)m_Comp.Document.Configurations[Name].Configuration;

        public override string Name 
        {
            get => m_Comp.Component.ConfigurationName;
            set => throw new NotSupportedException();
        }

        public override ISwDMConfiguration Configuration => m_Creator.Element;

        public override object Dispatch => m_Creator.Element;

        public override bool IsCommitted => m_Creator.IsCreated;

        public override void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);
    }

    internal class SwDmSubComponentCollection : SwDmComponentCollection
    {
        private readonly SwDmComponent m_ParentComp;

        internal SwDmSubComponentCollection(SwDmComponent parentComp,
            SwDmAssembly parentAssm, ISwDmConfiguration conf) 
            : base(parentAssm, conf)
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
}

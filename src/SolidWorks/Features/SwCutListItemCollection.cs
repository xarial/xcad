//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Features;
using Xarial.XCad.Base;
using Xarial.XCad.SolidWorks.Documents;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using Xarial.XCad.SolidWorks.Documents.Exceptions;
using Xarial.XCad.SolidWorks.Documents.EventHandlers;
using Xarial.XCad.Features.Delegates;
using System.Threading;
using Xarial.XCad.Toolkit.Utils;
using Xarial.XCad.SolidWorks.Enums;

namespace Xarial.XCad.SolidWorks.Features
{
    /// <summary>
    /// SOLIDWORKS specific cut-list items collection
    /// </summary>
    public interface ISwCutListItemCollection : IXCutListItemRepository
    {
        /// <summary>
        /// Returns cut-lists in the unordered order
        /// </summary>
        /// <remarks>This method may have better performance benefits</remarks>
        IEnumerable<ISwCutListItem> Unordered { get; }
    }

    internal abstract class SwCutListItemCollection : ISwCutListItemCollection
    {
        public abstract event CutListRebuildDelegate CutListRebuild;

        public IXCutListItem this[string name] => RepositoryHelper.Get(this, name);

        public int Count => IterateCutLists(false).Count();

        public IEnumerable<ISwCutListItem> Unordered => IterateCutLists(false);

        public bool AutomaticCutList 
        {
            get => OwnerPart.Model.Extension.GetUserPreferenceToggle(
                (int)swUserPreferenceToggle_e.swWeldmentEnableAutomaticCutList,
                (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);
            set => OwnerPart.Model.Extension.SetUserPreferenceToggle(
                (int)swUserPreferenceToggle_e.swWeldmentEnableAutomaticCutList,
                (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, value);
        }

        public bool AutomaticUpdate
        {
            get => OwnerPart.Model.Extension.GetUserPreferenceToggle(
                (int)swUserPreferenceToggle_e.swWeldmentEnableAutomaticUpdate,
                (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);
            set => OwnerPart.Model.Extension.SetUserPreferenceToggle(
                (int)swUserPreferenceToggle_e.swWeldmentEnableAutomaticUpdate,
                (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, value);
        }

        public void AddRange(IEnumerable<IXCutListItem> ents, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public IEnumerator<IXCutListItem> GetEnumerator()
            => IterateCutLists(true).GetEnumerator();

        public void RemoveRange(IEnumerable<IXCutListItem> ents, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public bool TryGet(string name, out IXCutListItem ent)
        {
            foreach (var cutList in IterateCutLists(false))
            {
                if (string.Equals(cutList.Name, name, StringComparison.CurrentCultureIgnoreCase)) 
                {
                    ent = cutList;
                    return true;
                }
            }

            ent = null;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) 
            => RepositoryHelper.FilterDefault(this, filters, reverseOrder);

        protected abstract IEnumerable<ISwCutListItem> IterateCutLists(bool ordered);

        public T PreCreate<T>() where T : IXCutListItem => throw new NotImplementedException();

        public bool Update()
        {
            if (FeatureManager.TryGetSolidBodyFeature(out var solidBodyFeat))
            {
                var solidBodyFolder = (IBodyFolder)solidBodyFeat.GetSpecificFeature2();
                return solidBodyFolder.UpdateCutList();
            }
            else 
            {
                throw new Exception("No solid body folder found");
            }
        }

        protected abstract SwFeatureManager FeatureManager { get; }
        protected abstract SwPart OwnerPart { get; }
    }

    internal class SwPartCutListItemCollection : SwCutListItemCollection
    {
        private readonly SwPartConfiguration m_Conf;
        private readonly SwPart m_Part;
        private readonly CutListRebuildEventsHandler m_CutListRebuild;

        protected override SwFeatureManager FeatureManager => (SwFeatureManager)m_Part.Features;

        protected override SwPart OwnerPart => m_Part;

        public override event CutListRebuildDelegate CutListRebuild
        {
            add
            {
                m_CutListRebuild.Attach(value);
            }
            remove
            {
                m_CutListRebuild.Detach(value);
            }
        }

        internal SwPartCutListItemCollection(SwPartConfiguration conf, SwPart part) 
        {
            m_Conf = conf;
            m_Part = part;

            m_CutListRebuild = new CutListRebuildEventsHandler(part, this);
        }

        protected override IEnumerable<ISwCutListItem> IterateCutLists(bool ordered)
        {
            if (m_Part.OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2024) && !ordered)
            {
                var cutListItems = (object[])m_Conf.Configuration.GetCutListItems();

                if (cutListItems != null)
                {
                    foreach (ICutListItem cutListItem in cutListItems)
                    {
                        var cutList = m_Part.CreateObjectFromDispatch<SwCutListItem>(cutListItem);
                        cutList.SetParent(m_Part, m_Conf);
                        yield return cutList;
                    }
                }
            }
            else
            {
                var activeConf = m_Part.Configurations.Active;

                var checkedConfigsConflict = false;

                foreach (var cutList in FeatureManager.IterateCutListFeatures(m_Part, activeConf))
                {
                    if (!checkedConfigsConflict)
                    {
                        if (activeConf.Name != m_Conf.Configuration.Name)
                        {
                            throw new ConfigurationSpecificCutListNotSupportedException();
                        }

                        checkedConfigsConflict = true;
                    }

                    yield return cutList;
                }
            }
        }
    }

    internal class SwPartComponentCutListItemCollection : SwCutListItemCollection
    {
        private readonly SwPartComponent m_Comp;

        private readonly Lazy<CutListRebuildEventsHandler> m_CutListRebuildLazy;

        protected override SwFeatureManager FeatureManager => (SwFeatureManager)m_Comp.Features;

        protected override SwPart OwnerPart => (SwPart)m_Comp.ReferencedDocument;

        public override event CutListRebuildDelegate CutListRebuild
        {
            add
            {
                m_CutListRebuildLazy.Value.Attach(value);
            }
            remove
            {
                m_CutListRebuildLazy.Value.Detach(value);
            }
        }

        internal SwPartComponentCutListItemCollection(SwPartComponent comp) 
        {
            m_Comp = comp;
            m_CutListRebuildLazy = new Lazy<CutListRebuildEventsHandler>(() => new CutListRebuildEventsHandler(OwnerPart, this));
        }

        protected override IEnumerable<ISwCutListItem> IterateCutLists(bool ordered)
        {
            var refConf = (ISwConfiguration)m_Comp.ReferencedConfiguration;
            var refDoc = m_Comp.ReferencedDocument;

            if (refDoc is ISwPart)
            {
                if (refDoc.OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2024) && refConf.IsCommitted && !ordered)
                {
                    var cutListItems = (object[])refConf.Configuration.GetCutListItems();

                    if (cutListItems != null)
                    {
                        foreach (ICutListItem cutListItem in cutListItems)
                        {
                            var compCutListItem = m_Comp.Component.GetCorresponding(cutListItem);
                            if (compCutListItem != null)
                            {
                                var cutList = refDoc.CreateObjectFromDispatch<SwCutListItem>(cutListItem);
                                cutList.SetParent(refDoc, refConf);
                                yield return cutList;
                            }
                            else 
                            {
                                throw new Exception("Failed to get corresponding cut list item");
                            }
                        }
                    }
                }
                else
                {
                    if (refDoc.IsCommitted)
                    {
                        foreach (var cutList in FeatureManager.IterateCutListFeatures(refDoc, refConf))
                        {
                            yield return cutList;
                        }
                    }
                }
            }
        }
    }
}

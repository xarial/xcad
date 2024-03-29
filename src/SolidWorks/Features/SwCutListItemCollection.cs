﻿//*********************************************************************
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

namespace Xarial.XCad.SolidWorks.Features
{
    internal abstract class SwCutListItemCollection : IXCutListItemRepository
    {
        public abstract event CutListRebuildDelegate CutListRebuild;

        public IXCutListItem this[string name] => RepositoryHelper.Get(this, name);

        public int Count => IterateCutLists().Count();

        public void AddRange(IEnumerable<IXCutListItem> ents, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public IEnumerator<IXCutListItem> GetEnumerator()
            => IterateCutLists().GetEnumerator();

        public void RemoveRange(IEnumerable<IXCutListItem> ents, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public bool TryGet(string name, out IXCutListItem ent)
        {
            foreach (var cutList in IterateCutLists())
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

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) => RepositoryHelper.FilterDefault(this, filters, reverseOrder);

        protected abstract IEnumerable<IXCutListItem> IterateCutLists();

        public T PreCreate<T>() where T : IXCutListItem => throw new NotImplementedException();
    }

    internal class SwPartCutListItemCollection : SwCutListItemCollection
    {
        private readonly ISwPartConfiguration m_Conf;
        private readonly ISwPart m_Part;
        private readonly CutListRebuildEventsHandler m_CutListRebuild;

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

        internal SwPartCutListItemCollection(ISwPartConfiguration conf, SwPart part) 
        {
            m_Conf = conf;
            m_Part = part;

            m_CutListRebuild = new CutListRebuildEventsHandler(part, this);
        }

        protected override IEnumerable<IXCutListItem> IterateCutLists()
        {
            var part = m_Part.Part;

            IEnumerable<IBody2> IterateBodies() =>
                (part.GetBodies2((int)swBodyType_e.swSolidBody, false) as object[] ?? new object[0]).Cast<IBody2>();

            if (part.IsWeldment()
                || IterateBodies().Any(b => b.IsSheetMetal()))
            {
                var activeConf = m_Part.Configurations.Active;

                var checkedConfigsConflict = false;

                foreach (var cutList in ((SwFeatureManager)m_Part.Features).IterateCutLists(m_Part, activeConf))
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
        private readonly SwComponent m_Comp;

        private readonly Lazy<CutListRebuildEventsHandler> m_CutListRebuildLazy;

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
            m_CutListRebuildLazy = new Lazy<CutListRebuildEventsHandler>(() => new CutListRebuildEventsHandler((SwPart)comp.ReferencedDocument, this));
        }

        protected override IEnumerable<IXCutListItem> IterateCutLists()
        {
            var refConf = (ISwConfiguration)m_Comp.ReferencedConfiguration;
            var refDoc = m_Comp.ReferencedDocument;

            if (refDoc is ISwPart)
            {
                IEnumerable<IBody2> IterateBodies() =>
                    (m_Comp.Component.GetBodies3((int)swBodyType_e.swSolidBody, out _) as object[] ?? new object[0]).Cast<IBody2>();

                if ((refDoc.IsCommitted && (refDoc.Model as IPartDoc).IsWeldment()) || IterateBodies().Any(b => b.IsSheetMetal()))
                {
                    foreach (var cutList in ((SwFeatureManager)m_Comp.Features).IterateCutLists(refDoc, refConf))
                    {
                        yield return cutList;
                    }
                }
            }
        }
    }
}

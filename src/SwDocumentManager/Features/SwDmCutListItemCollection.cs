//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
using Xarial.XCad.SwDocumentManager.Documents;
using SolidWorks.Interop.swdocumentmgr;
using Xarial.XCad.SwDocumentManager.Exceptions;
using Xarial.XCad.Features.Delegates;
using System.Threading;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SwDocumentManager.Features
{
    internal class SwDmCutListItemCollection : IXCutListItemRepository
    {
        #region Not Supported

        public event CutListRebuildDelegate CutListRebuild 
        {
            add => throw new NotSupportedException();
            remove => throw new NotSupportedException();
        }
        public T PreCreate<T>() where T : IXCutListItem => throw new NotSupportedException();
        #endregion

        private readonly ISwDmPartConfiguration m_Conf;
        private readonly SwDmPart m_Part;

        internal SwDmCutListItemCollection(ISwDmPartConfiguration conf, SwDmPart part) 
        {
            m_Conf = conf;
            m_Part = part;
        }

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

        private IEnumerable<IXCutListItem> IterateCutLists() 
        {
            object[] cutListItems = null;

            if (m_Part.IsVersionNewerOrEqual(SwDmVersion_e.Sw2019))
            {
                cutListItems = ((ISwDMConfiguration16)m_Conf.Configuration).GetCutListItems() as object[];
            }
            else
            {
                if (m_Conf.Configuration.Equals(m_Part.Configurations.Active.Configuration))
                {
                    cutListItems = ((ISwDMDocument13)m_Part.Document).GetCutListItems2() as object[];
                }
                else
                {
                    throw new ConfigurationCutListIsNotSupported();
                }
            }

            if (cutListItems != null)
            {
                foreach (var cutList in cutListItems.Cast<ISwDMCutListItem2>()
                    .Select(c => new SwDmCutListItem(c, m_Part, m_Conf)))
                {
                    yield return cutList;
                }
            }
        }
    }
}

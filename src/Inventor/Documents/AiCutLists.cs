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
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Features;
using Xarial.XCad.Features.Delegates;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.Inventor.Documents
{
    internal class AiCutLists : IXCutListItemRepository
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IXCutListItem this[string name] => m_RepoHelper.Get(name);

        public int Count => EnumerateCutLists().Count();

        public bool AutomaticCutList { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool AutomaticUpdate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event CutListRebuildDelegate CutListRebuild;

        private readonly RepositoryHelper<IXCutListItem> m_RepoHelper;

        internal AiCutLists() 
        {
            m_RepoHelper = new RepositoryHelper<IXCutListItem>(this);
        }

        public void AddRange(IEnumerable<IXCutListItem> ents, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters)
            => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public IEnumerator<IXCutListItem> GetEnumerator() => EnumerateCutLists().GetEnumerator();

        public T PreCreate<T>() where T : IXCutListItem
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<IXCutListItem> ents, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public bool TryGet(string name, out IXCutListItem ent) => m_RepoHelper.TryFindByName(name, out ent);

        private IEnumerable<IXCutListItem> EnumerateCutLists() 
        {
            yield break;
        }

        public bool Update()
        {
            throw new NotImplementedException();
        }
    }
}

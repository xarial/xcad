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

        public IXCutListItem this[string name] => RepositoryHelper.Get(this, name);

        public int Count => EnumerateCutLists().Count();

        public event CutListRebuildDelegate CutListRebuild;

        public void AddRange(IEnumerable<IXCutListItem> ents, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters)
            => RepositoryHelper.FilterDefault(this, filters, reverseOrder);

        public IEnumerator<IXCutListItem> GetEnumerator() => EnumerateCutLists().GetEnumerator();

        public T PreCreate<T>() where T : IXCutListItem
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<IXCutListItem> ents, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public bool TryGet(string name, out IXCutListItem ent) => RepositoryHelper.TryFindByName(this, name, out ent);

        private IEnumerable<IXCutListItem> EnumerateCutLists() 
        {
            yield break;
        }
    }
}

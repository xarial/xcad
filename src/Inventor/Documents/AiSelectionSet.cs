using Inventor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Graphics;

namespace Xarial.XCad.Inventor.Documents
{
    internal class AiSelectionSet : IXSelectionRepository
    {
        public IXSelObject this[string name] => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public event NewSelectionDelegate NewSelection;
        public event ClearSelectionDelegate ClearSelection;

        private readonly AiDocument m_Doc;

        private SelectSet SelectSet => m_Doc.Document.SelectSet;

        internal AiSelectionSet(AiDocument doc) 
        {
            m_Doc = doc;
        }

        public void AddRange(IEnumerable<IXSelObject> ents, CancellationToken cancellationToken)
        {
            var coll = m_Doc.OwnerApplication.Application.TransientObjects.CreateObjectCollection();

            foreach (IAiSelObject ent in ents)
            {
                if (ent.Dispatch is IAiArtificialEntity) 
                {
                    throw new NotSupportedException();
                }

                coll.Add(ent.Dispatch);
            }

            SelectSet.SelectMultiple(coll);
        }

        public void Clear() => SelectSet.Clear();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IXSelObject> GetEnumerator()
        {
            foreach (var disp in SelectSet) 
            {
                yield return m_Doc.OwnerApplication.CreateObjectFromDispatch<AiSelObject>(disp, m_Doc);
            }
        }

        public T PreCreate<T>() where T : IXSelObject
        {
            throw new NotImplementedException();
        }

        public IXSelCallout PreCreateCallout()
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<IXSelObject> ents, CancellationToken cancellationToken)
        {
            foreach (IAiSelObject ent in ents) 
            {
                SelectSet.Remove(ent.Dispatch);
            }
        }

        public void ReplaceRange(IEnumerable<IXSelObject> ents, CancellationToken cancellationToken)
        {
            Clear();
            AddRange(ents, cancellationToken);
        }

        public bool TryGet(string name, out IXSelObject ent)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

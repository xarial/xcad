using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Annotations;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwAnnotationCollection : IXAnnotationRepository 
    {
    }

    internal class SwAnnotationCollection : ISwAnnotationCollection
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly SwDocument m_Doc;

        public SwAnnotationCollection(SwDocument doc) 
        {
            m_Doc = doc;
        }

        public IXAnnotation this[string name] => RepositoryHelper.Get(this, name);

        public int Count => m_Doc.Model.Extension.GetAnnotationCount();

        public void AddRange(IEnumerable<IXAnnotation> ents, CancellationToken cancellationToken)
            => RepositoryHelper.AddRange(ents, cancellationToken);

        public IEnumerator<IXAnnotation> GetEnumerator()
        {
            //TODO: add support for drawings

            var ann = m_Doc.Model.IGetFirstAnnotation2();

            while (ann != null) 
            {
                yield return m_Doc.CreateObjectFromDispatch<ISwAnnotation>(ann);

                ann = ann.IGetNext2();
            }
        }

        public T PreCreate<T>() where T : IXAnnotation
            => RepositoryHelper.PreCreate<IXAnnotation, T>(this,
                () => new SwNote(null, m_Doc, m_Doc.OwnerApplication));

        public void RemoveRange(IEnumerable<IXAnnotation> ents, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public bool TryGet(string name, out IXAnnotation ent)
            => throw new NotSupportedException();
    }
}

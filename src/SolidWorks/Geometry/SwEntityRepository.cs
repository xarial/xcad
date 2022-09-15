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
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwEntityRepository : IXEntityRepository, IXRepository<ISwEntity>
    {
    }

    internal abstract class SwEntityRepository : ISwEntityRepository
    {
        IXEntity IXRepository<IXEntity>.this[string name] => this[name];
        void IXRepository<IXEntity>.AddRange(IEnumerable<IXEntity> ents, CancellationToken cancellationToken)
            => AddRange(ents?.Cast<ISwEntity>(), cancellationToken);
        void IXRepository<IXEntity>.RemoveRange(IEnumerable<IXEntity> ents, CancellationToken cancellationToken)
            => RemoveRange(ents?.Cast<ISwEntity>(), cancellationToken);

        bool IXRepository<IXEntity>.TryGet(string name, out IXEntity ent) 
        {
            var res = TryGet(name, out var specEnt);
            ent = specEnt;
            return res;
        }
        T IXRepository<ISwEntity>.PreCreate<T>() => PreCreate<T>();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        IEnumerator<IXEntity> IEnumerable<IXEntity>.GetEnumerator() => GetEnumerator();

        public ISwEntity this[string name] => throw new NotSupportedException();
        public void AddRange(IEnumerable<ISwEntity> ents, CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public void RemoveRange(IEnumerable<ISwEntity> ents, CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public bool TryGet(string name, out ISwEntity ent)
            => throw new NotSupportedException();
        public T PreCreate<T>() where T : IXEntity
            => throw new NotSupportedException();

        public virtual int Count => SelectAllEntities().Count();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters)
        {
            bool faces;
            bool edges;
            bool vertices;

            if (filters?.Any() == true)
            {
                faces = false;
                edges = false;
                vertices = false;

                foreach (var filter in filters)
                {
                    faces = filter.Type == null || typeof(IXFace).IsAssignableFrom(filter.Type);
                    edges = filter.Type == null || typeof(IXEdge).IsAssignableFrom(filter.Type);
                    vertices = filter.Type == null || typeof(IXVertex).IsAssignableFrom(filter.Type);
                }
            }
            else
            {
                faces = true;
                edges = true;
                vertices = true;
            }

            foreach (var ent in RepositoryHelper.FilterDefault(SelectEntities(faces, edges, vertices), filters, reverseOrder))
            {
                yield return ent;
            }
        }

        public IEnumerator<ISwEntity> GetEnumerator() => SelectAllEntities().GetEnumerator();

        private IEnumerable<ISwEntity> SelectAllEntities() => SelectEntities(true, true, true);

        protected abstract IEnumerable<ISwEntity> SelectEntities(bool faces, bool edges, bool vertices);
    }
}
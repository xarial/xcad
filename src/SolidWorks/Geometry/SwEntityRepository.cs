//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
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
            => m_RepoHelper.RemoveAll(ents, cancellationToken);

        public bool TryGet(string name, out ISwEntity ent) 
            => m_RepoHelper.TryFindByName(name, out ent);
        
        public T PreCreate<T>() where T : IXEntity
            => throw new NotSupportedException();

        public virtual int Count => IterateAllEntities().Count();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters)
        {
            bool faces;
            bool edges;
            bool vertices;
            bool silhouetteEdges;

            if (filters?.Any() == true)
            {
                faces = false;
                edges = false;
                vertices = false;
                silhouetteEdges = false;

                foreach (var filter in filters)
                {
                    faces = filter.Type == null || typeof(IXFace).IsAssignableFrom(filter.Type);
                    edges = filter.Type == null || typeof(IXEdge).IsAssignableFrom(filter.Type);
                    vertices = filter.Type == null || typeof(IXVertex).IsAssignableFrom(filter.Type);
                    silhouetteEdges = filter.Type == null || typeof(IXSilhouetteEdge).IsAssignableFrom(filter.Type);
                }
            }
            else
            {
                faces = true;
                edges = true;
                vertices = true;
                silhouetteEdges = true;
            }

            foreach (var ent in m_RepoHelper.FilterDefault(IterateEntities(faces, edges, vertices, silhouetteEdges), filters, reverseOrder))
            {
                yield return ent;
            }
        }

        public IEnumerator<ISwEntity> GetEnumerator() => IterateAllEntities().GetEnumerator();

        private IEnumerable<ISwEntity> IterateAllEntities() => IterateEntities(true, true, true, true);

        private readonly RepositoryHelper<ISwEntity> m_RepoHelper;

        protected SwEntityRepository() 
        {
            m_RepoHelper = new RepositoryHelper<ISwEntity>(this);
        }

        protected abstract IEnumerable<ISwEntity> IterateEntities(bool faces, bool edges, bool vertices, bool silhouetteEdges);
    }
}
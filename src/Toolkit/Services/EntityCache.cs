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
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Base;

namespace Xarial.XCad.Toolkit.Services
{
    /// <summary>
    /// This service allows to manage the entities which are created from the uncommitted object
    /// </summary>
    /// <typeparam name="TEnt">Type of entity</typeparam>
    public class EntityCache<TEnt> : IXRepository<TEnt>
            where TEnt : IXTransaction
    {
        private readonly List<TEnt> m_Cache;

        private readonly Func<TEnt, string> m_NameProvider;

        /// <summary>
        /// Owner of the entities
        /// </summary>
        protected readonly IXTransaction m_Owner;

        /// <summary>
        /// Source repository of the cahce
        /// </summary>
        protected readonly IXRepository<TEnt> m_Repo;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="owner">Owner of the cache</param>
        /// <param name="repo">Repository with cache</param>
        /// <param name="nameProvider">Provider for the name of the item in the cache</param>
        public EntityCache(IXTransaction owner, IXRepository<TEnt> repo, Func<TEnt, string> nameProvider)
        {
            m_Owner = owner;
            m_Repo = repo;
            m_NameProvider = nameProvider;

            m_Cache = new List<TEnt>();
        }

        /// <summary>
        /// Entitis count in the cache
        /// </summary>
        public int Count => m_Cache.Count;

        /// <inheritdoc/>
        public TEnt this[string name] => m_Repo[name];

        /// <summary>
        /// Adds entities to the cache
        /// </summary>
        /// <param name="ents"></param>
        /// <param name="cancellationToken"></param>
        public void AddRange(IEnumerable<TEnt> ents, CancellationToken cancellationToken)
            => m_Cache.AddRange(ents);

        /// <summary>
        /// Iterates entities in the cache
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TEnt> GetEnumerator() => IterateEntities(m_Cache).GetEnumerator();

        /// <summary>
        /// Removes entities from the cache
        /// </summary>
        /// <param name="ents">Entities to remove</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public void RemoveRange(IEnumerable<TEnt> ents, CancellationToken cancellationToken)
        {
            foreach (var ent in ents)
            {
                if (!m_Cache.Remove(ent))
                {
                    throw new Exception($"Failed to remove '{m_NameProvider.Invoke(ent)}' from cache");
                }
            }
        }

        /// <summary>
        /// Tries get entity from the cache
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ent"></param>
        /// <returns></returns>
        public virtual bool TryGet(string name, out TEnt ent)
        {
            ent = m_Cache.FirstOrDefault(c => string.Equals(m_NameProvider.Invoke(c), name, StringComparison.CurrentCultureIgnoreCase));

            return ent != null;
        }

        /// <summary>
        /// Commits entitities in the cache
        /// </summary>
        /// <param name="cancellationToken"></param>
        public void Commit(CancellationToken cancellationToken)
        {
            try
            {
                CommitEntitiesFromCache(m_Cache, cancellationToken);
            }
            finally
            {
                m_Cache.Clear();
            }
        }

        /// <summary>
        /// Iterates entities in the cache
        /// </summary>
        /// <param name="ents">Current entities</param>
        /// <returns>Entities</returns>
        protected virtual IEnumerable<TEnt> IterateEntities(IReadOnlyList<TEnt> ents) => ents;
        
        /// <summary>
        /// Commiting entities in the cache
        /// </summary>
        /// <param name="ents">Entities to commit</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual void CommitEntitiesFromCache(IReadOnlyList<TEnt> ents, CancellationToken cancellationToken) 
        {
            if (ents.Any())
            {
                if (m_Owner.IsCommitted)
                {
                    m_Repo.AddRange(ents, cancellationToken);
                }
                else
                {
                    throw new Exception("Commit is only possible when the owner entity is committed");
                }
            }
        }

        /// <inheritdoc/>
        public T PreCreate<T>() where T : TEnt => m_Repo.PreCreate<T>();

        /// <inheritdoc/>
        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) => m_Repo.Filter(reverseOrder, filters);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

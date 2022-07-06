using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Base;

namespace Xarial.XCad.Toolkit.Services
{
    public class EntityCache<TEnt>
            where TEnt : IXTransaction
    {
        private readonly List<TEnt> m_Cache;

        private readonly Func<TEnt, string> m_NameProvider;
        protected readonly IXTransaction m_Owner;
        protected readonly IXRepository<TEnt> m_Repo;

        public EntityCache(IXTransaction owner, IXRepository<TEnt> repo, Func<TEnt, string> nameProvider)
        {
            m_Owner = owner;
            m_Repo = repo;
            m_NameProvider = nameProvider;

            m_Cache = new List<TEnt>();
        }

        public int Count => m_Cache.Count;

        public void AddRange(IEnumerable<TEnt> ents, CancellationToken cancellationToken)
            => m_Cache.AddRange(ents);

        public IEnumerator<TEnt> GetEnumerator() => IterateEntities(m_Cache).GetEnumerator();

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

        public bool TryGet(string name, out TEnt ent)
        {
            ent = m_Cache.FirstOrDefault(c => string.Equals(m_NameProvider.Invoke(c), name, StringComparison.CurrentCultureIgnoreCase));

            return ent != null;
        }

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

        protected virtual IEnumerable<TEnt> IterateEntities(IReadOnlyList<TEnt> ents) => ents;
        
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
    }
}

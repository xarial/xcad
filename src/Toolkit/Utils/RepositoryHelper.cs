//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.Toolkit.Utils
{
    /// <summary>
    /// Helper functions of <see cref="IXRepository"/>
    /// </summary>
    public class RepositoryHelper<TEnt>
        where TEnt : IXTransaction
    {
        private readonly IXRepository<TEnt> m_Repo;
        private readonly Lazy<IReadOnlyDictionary<Type, Func<TEnt>>> m_FactoriesLazy;

        private readonly Dictionary<Type, Func<TEnt>> m_Cache;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repo">Repository to wrap</param>
        /// <param name="factories">Create factories</param>
        public RepositoryHelper(IXRepository<TEnt> repo, params Expression<Func<TEnt>>[] factories)
        {
            m_Repo = repo;

            m_FactoriesLazy = new Lazy<IReadOnlyDictionary<Type, Func<TEnt>>>(() =>
            {
                var funcs = new Dictionary<Type, Func<TEnt>>();

                foreach (var factory in factories)
                {
                    var type = factory.Body.Type;

                    var func = factory.Compile();

                    funcs.Add(type, func);
                }

                return funcs;
            });

            m_Cache = new Dictionary<Type, Func<TEnt>>();
        }

        /// <summary>
        /// Helper tool to automatically create specific entities
        /// </summary>
        /// <typeparam name="TSpecEnt">Specific entity</typeparam>
        /// <returns>Specific entity</returns>
        /// <exception cref="EntityNotSupportedException"/>
        public TSpecEnt PreCreate<TSpecEnt>()
            where TSpecEnt : TEnt
        {
            if (!m_Cache.TryGetValue(typeof(TSpecEnt), out var fact))
            {
                foreach (var curFact in m_FactoriesLazy.Value)
                {
                    var type = curFact.Key;

                    if (typeof(TSpecEnt).IsAssignableFrom(type))
                    {
                        fact = curFact.Value;
                        m_Cache.Add(typeof(TSpecEnt), fact);
                        break;
                    }
                }
            }

            if (fact != null)
            {
                return (TSpecEnt)fact.Invoke();
            }
            else
            {
                throw new EntityNotSupportedException(typeof(TSpecEnt), m_FactoriesLazy.Value.Keys.ToArray());
            }
        }

        /// <summary>
        /// Removes the entities
        /// </summary>
        /// <param name="ents">Entities to remove</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="Exception">Thrown if entity is not selectable</exception>
        public void RemoveAll(IEnumerable<TEnt> ents, CancellationToken cancellationToken)
        {
            if (ents == null)
            {
                throw new ArgumentNullException(nameof(ents));
            }

            foreach (var ent in ents.ToArray())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (ent is IXSelObject)
                {
                    ((IXSelObject)ent).Delete();
                }
                else
                {
                    throw new Exception($"Only '{nameof(IXSelObject)}' entities can be removed");
                }
            }
        }

        /// <summary>
        /// Tries to find the <see cref="IHasName"/> entity in the repository
        /// </summary>
        /// <param name="name">Name of the entity</param>
        /// <param name="ent">Entity</param>
        /// <returns>True if found</returns>
        public bool TryFindByName(string name, out TEnt ent)
        {
            ent = (TEnt)m_Repo.OfType<IHasName>()
                    .FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.CurrentCultureIgnoreCase));

            return ent != null;
        }

        /// <summary>
        /// Gets the entity by name
        /// </summary>
        /// <param name="name">Name of the entity</param>
        /// <returns>Pointer to named entity</returns>
        /// <exception cref="EntityNotFoundException"/>
        public TEnt Get(string name)
        {
            if (m_Repo.TryGet(name, out TEnt ent))
            {
                return ent;
            }
            else
            {
                throw new EntityNotFoundException(name);
            }
        }

        /// <summary>
        /// Performs the default commiting of entities into repository one-by-one
        /// </summary>
        /// <param name="ents">Entities to add</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="OperationCanceledException"/>
        public void AddRange(IEnumerable<TEnt> ents, CancellationToken cancellationToken)
        {
            if (ents == null)
            {
                throw new ArgumentNullException(nameof(ents));
            }

            foreach (var ent in ents)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    ent.Commit(cancellationToken);
                }
                else
                {
                    throw new OperationCanceledException();
                }
            }
        }

        /// <summary>
        /// Performs the default filtering of the entities
        /// </summary>
        /// <param name="ents">Entities to filter</param>
        /// <param name="filters">Filters</param>
        /// <param name="reverseOrder">True to reverse the order</param>
        /// <returns>Filtered entities</returns>
        /// <exception cref="EntityNotFoundException"></exception>
        public IEnumerable<TEnt> FilterDefault(IEnumerable<TEnt> ents, RepositoryFilterQuery[] filters, bool reverseOrder)
        {
            var filteredEnts = new List<TEnt>();

            foreach (var ent in ents)
            {
                if (MatchesFilters(ent, filters))
                {
                    if (reverseOrder)
                    {
                        filteredEnts.Insert(0, ent);
                    }
                    else
                    {
                        yield return ent;
                    }
                }
            }

            foreach (var ent in filteredEnts)
            {
                yield return ent;
            }
        }

        /// <summary>
        /// Checks if the specified entity matches the filter
        /// </summary>
        /// <param name="ent">Entity to match</param>
        /// <param name="filters">Filters</param>
        /// <returns>True if entity matches the filter</returns>
        public bool MatchesFilters(TEnt ent, params RepositoryFilterQuery[] filters)
        {
            if (filters?.Any() == true)
            {
                foreach (var filter in filters)
                {
                    if (filter.Type == null || filter.Type.IsAssignableFrom(ent.GetType()))
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                return true;
            }
        }
    }
}

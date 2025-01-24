//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Features;

namespace Xarial.XCad.Toolkit.Utils
{
    /// <summary>
    /// Helper class for the <see cref="RepositoryHelper{TEnt}"/>
    /// </summary>
    /// <typeparam name="TEnt">Type of transaction entity</typeparam>
    public class TransactionFactory<TEnt>
        where TEnt : IXTransaction
    {
        /// <summary>
        /// Creates new factory
        /// </summary>
        /// <typeparam name="TSpecEnt">Specific transaction type</typeparam>
        /// <param name="creator">Creator for this transaction</param>
        /// <returns>Factory</returns>
        public static TransactionFactory<TEnt> Create<TSpecEnt>(Func<TSpecEnt> creator)
            where TSpecEnt : class, TEnt => new TransactionFactory<TEnt>(typeof(TSpecEnt), creator);

        /// <summary>
        /// Specific type of the transaction this factory creates
        /// </summary>
        public Type SpecificType { get; }
        private readonly Func<TEnt> m_Creator;

        /// <summary>
        /// Creates new instance of the transaction
        /// </summary>
        /// <typeparam name="TSpecEnt">Specific entity type</typeparam>
        /// <returns>New instances</returns>
        public TSpecEnt New<TSpecEnt>()
            where TSpecEnt : TEnt 
            => (TSpecEnt)m_Creator.Invoke();

        /// <summary>
        /// Checks if factory is compatible with this specific entity type
        /// </summary>
        /// <typeparam name="TSpecEnt">Specific entity type</typeparam>
        /// <returns>True if factory can be used to create an istance of the specified type</returns>
        public bool IsCompatible<TSpecEnt>()
            where TSpecEnt : TEnt
            => typeof(TSpecEnt).IsAssignableFrom(SpecificType);

        private TransactionFactory(Type specificType, Func<TEnt> creator)
        {
            SpecificType = specificType;
            m_Creator = creator;
        }
    }

    /// <summary>
    /// Helper functions of <see cref="IXRepository"/>
    /// </summary>
    public class RepositoryHelper<TEnt>
        where TEnt : IXTransaction
    {
        private readonly IXRepository<TEnt> m_Repo;
        private readonly TransactionFactory<TEnt>[] m_Factories;

        private readonly Dictionary<Type, TransactionFactory<TEnt>> m_Cache;

        private readonly Type[] m_SupportedFactoryTypes;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repo">Repository to wrap</param>
        /// <param name="factories">Create factories</param>
        public RepositoryHelper(IXRepository<TEnt> repo, params TransactionFactory<TEnt>[] factories) 
        {            
            m_Repo = repo;

            m_Factories = factories;

            m_SupportedFactoryTypes = m_Factories.Select(f => f.SpecificType).ToArray();

            m_Cache = new Dictionary<Type, TransactionFactory<TEnt>>();
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
                fact = m_Factories.FirstOrDefault(f => f.IsCompatible<TSpecEnt>());

                if (fact != null) 
                {
                    m_Cache.Add(typeof(TSpecEnt), fact);
                }
            }

            if (fact != null)
            {
                return fact.New<TSpecEnt>();
            }
            else
            {
                throw new EntityNotSupportedException(typeof(TSpecEnt), m_SupportedFactoryTypes);
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

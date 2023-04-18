//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Xarial.XCad.Base
{
    /// <summary>
    /// Represents the collection of elements
    /// </summary>
    public interface IXRepository : IEnumerable
    {
        /// <summary>
        /// Number of elements in the collection
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Filters the entities with the specified query
        /// </summary>
        /// <param name="reverseOrder">Reverse the order of results</param>
        /// <param name="filters">Filters</param>
        /// <returns>Filtered entities</returns>
        IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters);//TODO: potentially replace the Filter with the IQueryable
    }

    /// <summary>
    /// Represents the collection of specific elements
    /// </summary>
    /// <typeparam name="TEnt"></typeparam>
    public interface IXRepository<TEnt> : IXRepository, IEnumerable<TEnt>
        where TEnt : IXTransaction
    {
        /// <summary>
        /// Retrieves the element by name
        /// </summary>
        /// <param name="name">Name of the element</param>
        /// <returns>Pointer to element</returns>
        /// <remarks>This method should through an exception for missing element. Use <see cref="TryGet(string, out TEnt)"/>for a safe way getting the element</remarks>
        TEnt this[string name] { get; }

        /// <summary>
        /// Attempts to get element by name
        /// </summary>
        /// <param name="name">Name of the element</param>
        /// <param name="ent">Resulting element if exists or null otherwise</param>
        /// <returns>True if element exists</returns>
        bool TryGet(string name, out TEnt ent);

        /// <summary>
        /// Commits entities
        /// </summary>
        /// <param name="ents"></param>
        /// <param name="cancellationToken">Cancellation token</param>
        void AddRange(IEnumerable<TEnt> ents, CancellationToken cancellationToken);

        /// <summary>
        /// Removes specified enitites
        /// </summary>
        /// <param name="ents">Entities to remove</param>
        /// <param name="cancellationToken">Cancellation token</param>
        void RemoveRange(IEnumerable<TEnt> ents, CancellationToken cancellationToken);

        /// <summary>
        /// Pre-creates template object
        /// </summary>
        /// <typeparam name="T">Specific type of the template object</typeparam>
        /// <returns>Template object</returns>
        /// <remarks>Use <see cref="IXTransaction.Commit(CancellationToken)"/> or <see cref="IXRepository{TEnt}.AddRange(IEnumerable{TEnt}, CancellationToken)"/> to commit templates and create objects</remarks>
        T PreCreate<T>() where T : TEnt;
    }

    /// <summary>
    /// Filter of the repository in the <see cref="IXRepository.Filter(RepositoryFilterQuery[])"/>
    /// </summary>
    public class RepositoryFilterQuery 
    {
        /// <summary>
        /// Type of entity or null for all types
        /// </summary>
        public Type Type { get; set; }
    }
}
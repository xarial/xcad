//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.Base
{
    /// <summary>
    /// Provides additional methods for <see cref="IXRepository{TEnt}"/>
    /// </summary>
    public static class XRepositoryExtension
    {
        /// <summary>
        /// Adds entities to the repository without the cancellation token
        /// </summary>
        /// <typeparam name="TEnt">Type of entity</typeparam>
        /// <param name="repo">Target repository</param>
        /// <param name="ents">Entities</param>
        public static void AddRange<TEnt>(this IXRepository<TEnt> repo, IEnumerable<TEnt> ents)
            where TEnt : IXTransaction
            => repo.AddRange(ents, CancellationToken.None);

        /// <summary>
        /// Removes entities from the repository without the cancellation token
        /// </summary>
        /// <typeparam name="TEnt">Type of entity</typeparam>
        /// <param name="repo">Target repository</param>
        /// <param name="ents">Entities</param>
        public static void RemoveRange<TEnt>(this IXRepository<TEnt> repo, IEnumerable<TEnt> ents)
            where TEnt : IXTransaction
            => repo.RemoveRange(ents, CancellationToken.None);

        /// <inheritdoc/>
        public static void Add<TEnt>(this IXRepository<TEnt> repo, params TEnt[] ents)
            where TEnt : IXTransaction
            => repo.AddRange(ents, CancellationToken.None);

        /// <summary>
        /// Adds object one-by-one or as an array
        /// </summary>
        /// <typeparam name="TEnt">Type of entity</typeparam>
        /// <param name="repo">Target repository</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="ents">Entities to add</param>
        public static void Add<TEnt>(this IXRepository<TEnt> repo, CancellationToken cancellationToken, params TEnt[] ents)
            where TEnt : IXTransaction
            => repo.AddRange(ents, cancellationToken);

        /// <inheritdoc/>
        public static void Remove<TEnt>(this IXRepository<TEnt> repo, params TEnt[] ents)
            where TEnt : IXTransaction
            => repo.RemoveRange(ents, CancellationToken.None);

        /// <summary>
        /// Removes object one-by-one or as an array
        /// </summary>
        /// <typeparam name="TEnt">Type of entity</typeparam>
        /// <param name="repo">Target repository</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="ents">Entities to remove</param>
        public static void Remove<TEnt>(this IXRepository<TEnt> repo, CancellationToken cancellationToken, params TEnt[] ents)
            where TEnt : IXTransaction
            => repo.RemoveRange(ents, cancellationToken);

        /// <summary>
        /// Pre-creates default template
        /// </summary>
        /// <typeparam name="TEnt"></typeparam>
        /// <param name="repo">Repository</param>
        /// <returns>Entity template</returns>
        public static TEnt PreCreate<TEnt>(this IXRepository<TEnt> repo) where TEnt : IXTransaction
            => repo.PreCreate<TEnt>();

        /// <summary>
        /// Filters entities by type
        /// </summary>
        /// <typeparam name="TSpecificEnt">Entity type</typeparam>
        /// <param name="repo">Repository</param>
        /// <param name="reverseOrder">True to reverse order</param>
        /// <returns>Filtered entities</returns>
        public static IEnumerable<TSpecificEnt> Filter<TSpecificEnt>(this IXRepository repo, bool reverseOrder = false)
            => repo.Filter(reverseOrder, new RepositoryFilterQuery()
            {
                Type = typeof(TSpecificEnt)
            }).Cast<TSpecificEnt>();
    }
}
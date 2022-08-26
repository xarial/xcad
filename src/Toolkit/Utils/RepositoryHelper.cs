//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.Toolkit.Utils
{
    public static class RepositoryHelper
    {
        /// <summary>
        /// Helper tool to automatically create specific entities
        /// </summary>
        /// <typeparam name="TEnt">Generic entity</typeparam>
        /// <typeparam name="TSpecEnt">Specific entity</typeparam>
        /// <param name="repo">Repository</param>
        /// <param name="factories">Factories of the specific objects</param>
        /// <returns>Specific entity</returns>
        /// <exception cref="EntityNotSupportedException"/>
        public static TSpecEnt PreCreate<TEnt, TSpecEnt>(IXRepository<TEnt> repo, params Expression<Func<TEnt>>[] factories)
            where TEnt : IXTransaction
            where TSpecEnt : TEnt
        {
            var supportedTypes = new List<Type>();

            foreach (var factory in factories)
            {
                var type = factory.Body.Type;

                if (typeof(TSpecEnt).IsAssignableFrom(type))
                {
                    return (TSpecEnt)factory.Compile().Invoke();
                }

                supportedTypes.Add(type);
            }

            throw new EntityNotSupportedException(typeof(TSpecEnt), supportedTypes);
        }

        /// <summary>
        /// Gets the entity by name
        /// </summary>
        /// <typeparam name="TEnt">Type of entity</typeparam>
        /// <param name="repo">Target repository</param>
        /// <param name="name">Name of the entity</param>
        /// <returns>Pointer to named entity</returns>
        /// <exception cref="EntityNotFoundException"/>
        public static TEnt Get<TEnt>(IXRepository<TEnt> repo, string name)
            where TEnt : IXTransaction
        {
            if (repo.TryGet(name, out TEnt ent))
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
        /// <typeparam name="TEnt">Entity type</typeparam>
        /// <param name="repo">Repository</param>
        /// <param name="ents">Entities</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="OperationCanceledException"/>
        public static void AddRange<TEnt>(IEnumerable<TEnt> ents, CancellationToken cancellationToken)
            where TEnt : IXTransaction
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
    }
}

//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Exceptions;

namespace Xarial.XCad.Base
{
    /// <summary>
    /// Provides additional methods for <see cref="IXRepository{TEnt}"/>
    /// </summary>
    public static class IXRepositoryExtension
    {
        /// <summary>
        /// Adds object one-by-one or as an array
        /// </summary>
        /// <typeparam name="TEnt">Type of entity</typeparam>
        /// <param name="repo">Target repository</param>
        /// <param name="ents">Entities to add</param>
        public static void Add<TEnt>(this IXRepository<TEnt> repo, params TEnt[] ents)
            where TEnt : IXTransaction
        {
            repo.AddRange(ents);
        }

        /// <summary>
        /// Gets the entity by name
        /// </summary>
        /// <typeparam name="TEnt">Type of entity</typeparam>
        /// <param name="repo">Target repository</param>
        /// <param name="name">Name of the entity</param>
        /// <returns>Pointer to named entity</returns>
        /// <exception cref="EntityNotFoundException"/>
        public static TEnt Get<TEnt>(this IXRepository<TEnt> repo, string name)
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
    }
}
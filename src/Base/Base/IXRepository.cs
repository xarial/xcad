//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;

namespace Xarial.XCad.Base
{
    /// <summary>
    /// Represents the collection of elements
    /// </summary>
    /// <typeparam name="TEnt"></typeparam>
    public interface IXRepository<TEnt> : IEnumerable<TEnt>
        where TEnt : IXTransaction
    {
        /// <summary>
        /// Number of elements in the collection
        /// </summary>
        int Count { get; }

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
        void AddRange(IEnumerable<TEnt> ents);

        /// <summary>
        /// Removes specified enitites
        /// </summary>
        /// <param name="ents">Entities to remove</param>
        void RemoveRange(IEnumerable<TEnt> ents);
    }
}
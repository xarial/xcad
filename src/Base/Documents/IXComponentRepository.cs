//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents the collection of <see cref="IXComponent"/>
    /// </summary>
    public interface IXComponentRepository : IXRepository<IXComponent>
    {
        /// <summary>
        /// Returns the total count of components including all nested components
        /// </summary>
        int TotalCount { get; }
    }

    /// <summary>
    /// Additonal methods of <see cref="IXComponentRepository"/>
    /// </summary>
    public static class XComponentRepositoryExtension 
    {
        /// <summary>
        /// Returns all components, including children
        /// </summary>
        /// <param name="repo">Components repository</param>
        /// <returns>All components</returns>
        public static IEnumerable<IXComponent> TryFlatten(this IXComponentRepository repo) 
        {
            IEnumerator<IXComponent> enumer;

            try
            {
                enumer = repo.GetEnumerator();
            }
            catch 
            {
                yield break;
            }

            while (true) 
            {
                IXComponent comp;

                try
                {
                    if (!enumer.MoveNext())
                    {
                        break;
                    }

                    comp = enumer.Current;
                }
                catch 
                {
                    break;
                }

                yield return comp;

                IXComponentRepository children;

                var state = comp.State;

                if (!state.HasFlag(ComponentState_e.Suppressed) && !state.HasFlag(ComponentState_e.SuppressedIdMismatch))
                {
                    try
                    {
                        children = comp.Children;
                    }
                    catch 
                    {
                        children = null;
                    }
                }
                else
                {
                    children = null;
                }

                if (children != null)
                {
                    foreach (var subComp in TryFlatten(children))
                    {
                        yield return subComp;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a template for part component
        /// </summary>
        /// <returns>Part component template</returns>
        public static IXPartComponent PreCreatePartComponent(this IXComponentRepository repo) => repo.PreCreate<IXPartComponent>();

        /// <summary>
        /// Creates a template for assembly component
        /// </summary>
        /// <returns>Assembly component template</returns>
        public static IXAssemblyComponent PreCreateAssemblyComponent(this IXComponentRepository repo) => repo.PreCreate<IXAssemblyComponent>();
    }
}

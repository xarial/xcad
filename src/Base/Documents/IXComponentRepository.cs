//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents the collection of <see cref="IXComponent"/>
    /// </summary>
    public interface IXComponentRepository : IXRepository<IXComponent>
    {
    }

    /// <summary>
    /// Additonal methods of <see cref="IXComponentRepository"/>
    /// </summary>
    public static class IXComponentRepositoryExtension 
    {
        /// <summary>
        /// Returns all components, including children
        /// </summary>
        /// <param name="repo">Components repository</param>
        /// <returns>All components</returns>
        public static IEnumerable<IXComponent> Flatten(this IXComponentRepository repo) 
        {
            foreach (var comp in repo) 
            {
                foreach (var subComp in Flatten(comp.Children)) 
                {
                    yield return subComp;
                }

                yield return comp;
            }
        }
    }
}

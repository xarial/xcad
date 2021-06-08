//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xarial.XCad.Documents.Extensions
{
    public static class IXDocumentExtension
    {
        /// <summary>
        /// Returns all dependencies including nested dependencies
        /// </summary>
        /// <param name="doc">Input document</param>
        /// <returns>All dependencies</returns>
        public static IEnumerable<IXDocument3D> GetAllDependencies(this IXDocument doc)
            => EnumerateDependencies(doc, new List<string>());

        private static IEnumerable<IXDocument3D> EnumerateDependencies(IXDocument doc, List<string> usedPaths) 
        {
            IXDocument3D[] deps = null;

            try
            {
                deps = doc.Dependencies;
            }
            catch 
            {
            }

            foreach (var dep in deps ?? new IXDocument3D[0])
            {
                if (!usedPaths.Contains(dep.Path, StringComparer.CurrentCultureIgnoreCase))
                {
                    usedPaths.Add(dep.Path);
                    yield return dep;

                    foreach (var childDep in EnumerateDependencies(dep, usedPaths)) 
                    {
                        yield return childDep;
                    }
                }
            }
        }
    }
}

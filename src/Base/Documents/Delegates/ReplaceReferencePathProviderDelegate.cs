using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Documents.Delegates
{
    /// <summary>
    /// Delegate used in the <see cref="XDocumentDependenciesExtension.ReplaceAll(IXDocumentDependencies, ReplaceReferencePathProviderDelegate, Func{string, string})"/>
    /// </summary>
    /// <param name="srcPath">Path to be replaced</param>
    /// <returns>Replacement path (can be the same if reference does not need to be replaced)</returns>
    public delegate string ReplaceReferencePathProviderDelegate(string srcPath);
}

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
using System.Threading.Tasks;

namespace Xarial.XCad.Documents.Delegates
{
    /// <summary>
    /// Delegate used in the <see cref="XDocumentDependenciesExtension.ReplaceAll(IXDocumentDependencies, ReplaceReferencePathProviderDelegate, Func{string, string})"/>
    /// when target file reference cannot be resolved
    /// </summary>
    /// <param name="requestingFilePath">Parent file path which is requesting this reference</param>
    /// <param name="missingPath">Path that was not found in the rename map</param>
    /// <returns>Replacement path (can be the same if reference does not need to be replaced)</returns>
    /// <remarks>This happens if CAD system was able to automatically resolve new path and replacing is not required (usually when <see cref="IXDocumentDependencies.Cached"/> is set to false</remarks>
    public delegate string ReplaceReferencePathMissingConflictResolverDelegate(string requestingFilePath, string missingPath);
}

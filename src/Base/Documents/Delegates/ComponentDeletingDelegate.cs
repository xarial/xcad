//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents.Structures;

namespace Xarial.XCad.Documents.Delegates
{
    /// <summary>
    /// Delegate of <see cref="IXAssembly.ComponentDeleting"/> notification
    /// </summary>
    /// <param name="assembly">Assembly where component is being deleted</param>
    /// <param name="component">Component being deleted from the assembly</param>
    /// <param name="args">Deleting arguments</param>
    public delegate void ComponentDeletingDelegate(IXAssembly assembly, IXComponent component, ItemDeleteArgs args);
}

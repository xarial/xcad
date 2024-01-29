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
    /// Delegate of <see cref="IXAssembly.ComponentDeleted"/> notification
    /// </summary>
    /// <param name="assembly">Assembly where component is deleted</param>
    /// <param name="component">Component deleted from the assembly. The pointer to the component may be disconnected from the client</param>
    public delegate void ComponentDeletedDelegate(IXAssembly assembly, IXComponent component);
}

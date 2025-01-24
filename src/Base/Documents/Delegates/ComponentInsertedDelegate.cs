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

namespace Xarial.XCad.Documents.Delegates
{
    /// <summary>
    /// Delegate of <see cref="IXAssembly.ComponentInserted"/> notification
    /// </summary>
    /// <param name="assembly">Assembly where component is inserted</param>
    /// <param name="component">Component inserted into the assembly</param>
    public delegate void ComponentInsertedDelegate(IXAssembly assembly, IXComponent component);
}

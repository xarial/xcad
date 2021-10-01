//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents.Attributes;

namespace Xarial.XCad.Documents.Enums
{
    /// <summary>
    /// Scope of the handler used in <see cref="DocumentHandlerScopeAttribute"/>
    /// </summary>
    [Flags]
    public enum DocumentHandlerScope_e
    {
        /// <summary>
        /// Part documents (<see cref="IXPart"/>)
        /// </summary>
        Part = 1,

        /// <summary>
        /// Assembly documents (<see cref="IXAssembly"/>)
        /// </summary>
        Assembly = 2,

        /// <summary>
        /// Drawing documents (<see cref="IXDrawing"/>)
        /// </summary>
        Drawing = 4
    }
}

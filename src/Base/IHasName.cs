//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad
{
    /// <summary>
    /// Indicates that object has name
    /// </summary>
    public interface IHasName : IXObject
    {
        /// <summary>
        /// Name of this element
        /// </summary>
        string Name { get; set; }
    }
}

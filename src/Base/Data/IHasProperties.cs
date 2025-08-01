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

namespace Xarial.XCad.Data
{
    /// <summary>
    /// Indicates that the object exposes properties
    /// </summary>
    public interface IHasProperties
    {
        /// <summary>
        /// Properties repository
        /// </summary>
        IXPropertyRepository Properties { get; }
    }
}

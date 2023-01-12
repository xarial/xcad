//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad
{
    /// <summary>
    /// Represents the material
    /// </summary>
    public interface IXMaterial
    {
        /// <summary>
        /// Material database
        /// </summary>
        string Database { get; }

        /// <summary>
        /// Name of the material
        /// </summary>
        string Name { get; }
    }
}

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

namespace Xarial.XCad.Annotations
{
    /// <summary>
    /// Represents the information about cross hatch
    /// </summary>
    public interface IXCrossHatch
    {
        /// <summary>
        /// Name of the hatch
        /// </summary>
        string Name { get; }

        /// <summary>
        /// hatch angle
        /// </summary>
        double Angle { get; }

        /// <summary>
        /// Hatch scale
        /// </summary>
        double Scale { get; }
    }
}

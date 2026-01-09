//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents display state
    /// </summary>
    public interface IXDisplayState : IXObject, IHasName
    {
        /// <summary>
        /// Appearances in this display state
        /// </summary>
        IXAppearanceRepository Appearances { get; }
    }
}

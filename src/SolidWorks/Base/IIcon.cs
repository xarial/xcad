//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using System.Drawing;

namespace Xarial.XCad.SolidWorks.Base
{
    /// <summary>
    /// Represents the specific icon descriptor
    /// </summary>
    public interface IIcon
    {
        /// <summary>
        /// Indicates that this icon is permanent and should not be removed on dispose
        /// </summary>
        bool IsPermanent { get; }

        /// <summary>
        /// Transparency key to be applied to transparent color
        /// </summary>
        Color TransparencyKey { get; }

        /// <summary>
        /// List of required icon sizes
        /// </summary>
        /// <returns></returns>
        IEnumerable<IIconSpec> GetIconSizes();
    }
}
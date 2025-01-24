//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Drawing;
using Xarial.XCad.Properties;
using Xarial.XCad.Reflection;
using Xarial.XCad.UI;

namespace Xarial.XCad
{
    /// <summary>
    /// Collection of default objects
    /// </summary>
    public static class Defaults
    {
        /// <summary>
        /// Default icon
        /// </summary>
        public static IXImage Icon
            => new BaseImage(Resources.default_icon);
    }
}
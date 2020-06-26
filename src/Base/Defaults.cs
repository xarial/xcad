//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Drawing;
using Xarial.XCad.Properties;
using Xarial.XCad.Reflection;
using Xarial.XCad.UI;

namespace Xarial.XCad
{
    public static class Defaults
    {
        public static IXImage Icon
        {
            get
            {
                return ResourceHelper.FromBytes(Resources.default_icon);
            }
        }
    }
}
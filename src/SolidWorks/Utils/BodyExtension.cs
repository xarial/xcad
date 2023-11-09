//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal static class BodyExtension
    {
        internal static IBody2 CreateCopy(this IBody2 body, SwApplication app) 
        {
            if (app.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2019))
            {
                return (IBody2)body.Copy2(true);
            }
            else
            {
                return (IBody2)body.Copy();
            }
        }
    }
}

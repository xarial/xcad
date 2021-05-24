//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.SolidWorks.Base;

namespace Xarial.XCad.SolidWorks.Services
{
    public interface IIconsCreator : IDisposable
    {
        string IconsFolder { get; set; }
        bool KeepIcons { get; set; }
        string[] ConvertIcon(IIcon icon);
        string[] ConvertIconsGroup(IIcon[] icons);
    }
}

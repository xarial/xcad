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

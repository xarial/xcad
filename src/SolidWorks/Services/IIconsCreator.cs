using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.SolidWorks.Base;

namespace Xarial.XCad.SolidWorks.Services
{
    public interface IIconsCreator : IDisposable
    {
        bool KeepIcons { get; set; }
        string[] ConvertIcon(IIcon icon);
        string[] ConvertIconsGroup(IIcon[] icons);
    }
}

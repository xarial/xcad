//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.ObjectModel;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls
{
    public interface IWpfPropertyManagerPageControlsHost : IWpfPropertyManagerPageControl
    {
        ObservableCollection<IWpfPropertyManagerPageControl> Controls { get; }
    }
}

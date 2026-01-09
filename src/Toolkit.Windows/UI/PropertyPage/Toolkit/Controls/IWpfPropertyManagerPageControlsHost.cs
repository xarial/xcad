using System.Collections.ObjectModel;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls
{
    public interface IWpfPropertyManagerPageControlsHost : IWpfPropertyManagerPageControl
    {
        ObservableCollection<IWpfPropertyManagerPageControl> Controls { get; }
    }
}

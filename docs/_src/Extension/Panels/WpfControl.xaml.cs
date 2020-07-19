using System.Windows.Controls;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documentation.Properties;

namespace Xarial.XCad.Documentation.Extension.Panels
{
    [Title("WPF Control")]
    [Icon(typeof(Resources), nameof(Properties.Resources.wpf_icon))]
    public partial class WpfControl : UserControl
    {
        public WpfControl()
        {
            InitializeComponent();
        }
    }
}

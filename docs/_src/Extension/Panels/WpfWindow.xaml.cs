using System.Windows;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documentation.Properties;

namespace Xarial.XCad.Documentation.Extension.Panels
{
    [Title("WPF Window")]
    [Icon(typeof(Resources), nameof(Properties.Resources.wpf_icon))]
    public partial class WpfWindow : Window
    {
        public WpfWindow()
        {
            InitializeComponent();
        }
    }
}

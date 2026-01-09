using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Xarial.XCad.BimVision.UI.PropertyPage.Toolkit.Templates
{
    public partial class PropertyManagerPageLayout : UserControl
    {
        internal event Action<bool> ClosingClicked;
        internal event Action WhatsNew;
        internal event Action Help;

        public PropertyManagerPageLayout()
        {
            InitializeComponent();
        }

        private void OnOkClick(object sender, MouseButtonEventArgs e)
        {
            ClosingClicked?.Invoke(false);
        }

        private void OnCancelClick(object sender, MouseButtonEventArgs e)
        {
            ClosingClicked?.Invoke(true);
        }

        private void OnWhatsNewClick(object sender, MouseButtonEventArgs e)
        {
            WhatsNew?.Invoke();
        }

        private void OnHelpClick(object sender, MouseButtonEventArgs e)
        {
            Help?.Invoke();
        }
    }
}

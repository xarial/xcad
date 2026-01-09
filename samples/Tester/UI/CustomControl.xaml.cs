using System;
using System.Collections.Generic;
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
using Xarial.XCad.UI.PropertyPage;

namespace Tester.UI
{
    public partial class CustomControl : UserControl, IXCustomControl
    {
        public event CustomControlValueChangedDelegate ValueChanged;

        public CustomControl()
        {
            InitializeComponent();
        }

        public object Value 
        {
            get => this.DataContext;
            set
            {
                this.DataContext = value;
                ValueChanged?.Invoke(this, value);
            }
        }
    }
}

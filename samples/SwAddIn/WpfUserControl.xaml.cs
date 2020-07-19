//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SwAddInExample.Properties;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xarial.XCad.Base.Attributes;

namespace SwAddInExample
{
    [Title("WPF User Control")]
    [Icon(typeof(Resources), nameof(Properties.Resources.xarial))]
    public partial class WpfUserControl : UserControl
    {
        public WpfUserControl()
        {
            InitializeComponent();
        }
    }
}

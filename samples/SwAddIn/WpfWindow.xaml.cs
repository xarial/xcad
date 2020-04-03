//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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

namespace SwAddInExample
{
    public partial class WpfWindow : Window
    {
        public bool? IsOk { get; private set; }

        public WpfWindow()
        {
            InitializeComponent();
            IsOk = null;
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            IsOk = true;
            this.Close();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            IsOk = false;
            this.Close();
        }
    }
}

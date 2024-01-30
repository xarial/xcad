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
using Xarial.XCad.UI.PropertyPage;

namespace SwAddInExample
{
    [Title("WPF User Control")]
    [Icon(typeof(Resources), nameof(Properties.Resources.xarial))]
    public partial class WpfUserControl : UserControl, IDisposable, IXCustomControl
    {
        public event CustomControlValueChangedDelegate ValueChanged;

        private CustomControlDataContext m_Context;

        public WpfUserControl()
        {
            InitializeComponent();
            m_Context = new CustomControlDataContext();
            m_Context.ValueChanged += OnContextValueChanged;
            this.DataContext = m_Context;
        }

        public object Value 
        {
            get => m_Context.Value;
            set => m_Context.Value = (OptsFlag)value;
        }

        private void OnContextValueChanged(CustomControlDataContext sender, OptsFlag value)
        {
            ValueChanged?.Invoke(this, value);
        }

        public void Dispose()
        {
        }
    }
}

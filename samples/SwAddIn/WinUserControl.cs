//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Xarial.XCad.UI.PropertyPage;

namespace SwAddInExample
{
    public partial class WinUserControl : UserControl, IXCustomControl
    {
        public WinUserControl()
        {
            InitializeComponent();
        }

        public object Value { get; set; }

        public event Action<IXCustomControl, object> ValueChanged;
    }
}

#if NET461
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Xarial.XCad.UI.PropertyPage;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal class WpfCustomControl : IXCustomControl
    {
        public event Action<IXCustomControl, object> DataContextChanged;

        private readonly FrameworkElement m_Elem;

        internal WpfCustomControl(FrameworkElement elem) 
        {
            m_Elem = elem;
        }

        public object DataContext 
        {
            get => m_Elem.DataContext;
            set => m_Elem.DataContext = value;
        }
    }
}
#endif
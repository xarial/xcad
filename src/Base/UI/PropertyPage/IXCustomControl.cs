//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.UI.PropertyPage
{
    public interface IXCustomControl
    {
        event Action<IXCustomControl, object> DataContextChanged;
        object DataContext { get; set; }
    }
}

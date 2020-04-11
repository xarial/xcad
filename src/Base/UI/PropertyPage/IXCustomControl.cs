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

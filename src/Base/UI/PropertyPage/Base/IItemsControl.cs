using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI.PropertyPage.Structures;

namespace Xarial.XCad.UI.PropertyPage.Base
{
    public interface IItemsControl : IControl
    {
        ItemsControlItem[] Items { get; set; }
    }
}

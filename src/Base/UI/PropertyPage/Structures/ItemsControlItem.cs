//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xarial.XCad.UI.PropertyPage.Structures
{
    [DebuggerDisplay("{" + nameof(DisplayName) + "} [{" + nameof(Value) + "}]")]
    public class ItemsControlItem
    {
        public string DisplayName { get; set; }
        public object Value { get; set; }

        public ItemsControlItem() 
        {
        }

        public ItemsControlItem(object item)
        {
            Value = item;
            DisplayName = item?.ToString();
        }
    }
}

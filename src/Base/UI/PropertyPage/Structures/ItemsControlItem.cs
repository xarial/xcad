//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace Xarial.XCad.UI.PropertyPage.Structures
{
    /// <summary>
    /// Represents the item in the <see cref="Base.IItemsControl"/>
    /// </summary>
    [DebuggerDisplay("{" + nameof(DisplayName) + "} [{" + nameof(Value) + "}]")]
    public class ItemsControlItem
    {
        /// <summary>
        /// Display name of the item
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Value of the item
        /// </summary>
        public object Value { get; }

        public ItemsControlItem(object value, DisplayMemberMemberPath dispMembPath) 
            : this(value, dispMembPath.GetDisplayName(value))
        {
        }

        public ItemsControlItem(object value, string dispName)
        {
            Value = value;
            DisplayName = dispName;
        }
    }
}

//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xarial.XCad.Reflection;

namespace Xarial.XCad.UI.PropertyPage.Structures
{
    /// <summary>
    /// Represents the item in the <see cref="Base.IItemsControl"/>
    /// </summary>
    [DebuggerDisplay("{" + nameof(DisplayName) + "} [{" + nameof(Value) + "}]")]
    public class ItemsControlItem
    {
        private static string GetDisplayName(object value, string dispMembPath, out object prpOwner, out string prpName)
        {
            prpOwner = null;
            prpName = "";

            if (!string.IsNullOrEmpty(dispMembPath))
            {
                var prps = dispMembPath.Split('.');

                var curVal = value;

                for (int i = 0; i < prps.Length; i++)
                {
                    if (i == prps.Length - 1) 
                    {
                        prpOwner = curVal;
                        prpName = prps[i];
                    }

                    curVal = GetPropertyValue(curVal, prps[i]);
                }

                return curVal?.ToString() ?? "";
            }
            else
            {
                if (value != null)
                {
                    string dispName = "";

                    value.GetType().TryGetAttribute<DisplayNameAttribute>(a => dispName = a.DisplayName);

                    if (string.IsNullOrEmpty(dispName))
                    {
                        dispName = value.ToString();
                    }

                    return dispName;
                }
                else 
                {
                    return "";
                }
            }
        }

        private static string GetDescription(object value)
        {
            string desc = "";

            if (value != null)
            {
                value.GetType().TryGetAttribute<DescriptionAttribute>(a => desc = a.Description);
            }

            return desc;
        }

        private static object GetPropertyValue(object value, string prpName)
        {
            if (value != null)
            {
                var prp = value.GetType().GetProperty(prpName);

                if (prp != null)
                {
                    return prp.GetValue(value, null);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Notifies when the display name is changed
        /// </summary>
        public event Action<ItemsControlItem, string> DisplayNameChanged;

        /// <summary>
        /// Display name of the item
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Value of the item
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Description of the item
        /// </summary>
        public string Description { get; }

        private readonly INotifyPropertyChanged m_DisplayNamePrpOwner;
        private readonly string m_DisplayNamePrpName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Item value</param>
        /// <param name="dispMembPath">Display member path of the item</param>
        public ItemsControlItem(object value, string dispMembPath) 
            : this(value, GetDisplayName(value, dispMembPath, out var prpOwner, out var prpName), GetDescription(value))
        {
            if (prpOwner is INotifyPropertyChanged) 
            {
                m_DisplayNamePrpOwner = (INotifyPropertyChanged)prpOwner;
                m_DisplayNamePrpName = prpName;

                m_DisplayNamePrpOwner.PropertyChanged += OnDisplayNamePropertyChanged;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Item value</param>
        /// <param name="dispName">Item display name</param>
        /// <param name="desc">Item description</param>
        public ItemsControlItem(object value, string dispName, string desc)
        {
            Value = value;
            DisplayName = dispName;
            Description = desc;
        }

        private void OnDisplayNamePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == m_DisplayNamePrpName)
            {
                DisplayNameChanged?.Invoke(this, GetPropertyValue(m_DisplayNamePrpOwner, m_DisplayNamePrpName)?.ToString());
            }
        }
    }
}

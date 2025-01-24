//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Reflection;

namespace Xarial.XCad.UI.Structures
{
    /// <summary>
    /// Defines the group of buttons
    /// </summary>
    public class ButtonGroupSpec
    {
        /// <summary>
        /// Title of the group
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Help text (tooltip) of the group
        /// </summary>
        public string Tooltip { get; set; }

        /// <summary>
        /// Group icon
        /// </summary>
        public IXImage Icon { get; set; }
    }

    internal static class ButtonGroupSpecExtension
    {
        internal static void InitFromEnum<TEnum>(this ButtonGroupSpec btnGrp)
            where TEnum : Enum
        {
            var cmdGroupType = typeof(TEnum);

            if (!cmdGroupType.TryGetAttribute<IconAttribute>(a => btnGrp.Icon = a.Icon))
            {
                btnGrp.Icon = Defaults.Icon;
            }

            if (!cmdGroupType.TryGetAttribute<DisplayNameAttribute>(a => btnGrp.Title = a.DisplayName))
            {
                btnGrp.Title = cmdGroupType.Name;
            }

            if (!cmdGroupType.TryGetAttribute<DescriptionAttribute>(a => btnGrp.Tooltip = a.Description))
            {
                btnGrp.Tooltip = cmdGroupType.Name;
            }
        }
    }
}

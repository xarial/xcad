using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Reflection;

namespace Xarial.XCad.UI.Structures
{
    public class ButtonGroupSpec
    {
        public string Title { get; set; }
        public string Tooltip { get; set; }
        public Image Icon { get; set; }
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

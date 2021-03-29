//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Reflection;
using Xarial.XCad.UI;
using Xarial.XCad.UI.Structures;
using Xarial.XCad.UI.TaskPane;
using Xarial.XCad.UI.TaskPane.Attributes;

namespace Xarial.XCad.Extensions
{
    public static class XExtensionExtension
    {
        public static IXEnumTaskPane<TControl, TEnum> CreateTaskPane<TControl, TEnum>(this IXExtension ext)
            where TEnum : Enum
        {
            var spec = new TaskPaneSpec();
            spec.InitFromEnum<TEnum>();
            spec.Buttons = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Select(
                c => 
                {
                    var btn = new TaskPaneEnumButtonSpec<TEnum>(Convert.ToInt32(c));
                    btn.InitFromEnum(c);
                    btn.Value = c;
                    c.TryGetAttribute<TaskPaneStandardIconAttribute>(a => btn.StandardIcon = a.StandardIcon);
                    return btn;
                }).ToArray();

            return new EnumTaskPane<TControl, TEnum>(ext.CreateTaskPane<TControl>(spec));
        }
    }
}

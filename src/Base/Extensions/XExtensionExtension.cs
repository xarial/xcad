//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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
    /// <summary>
    /// Additional methods for the <see cref="IXExtension"/>
    /// </summary>
    public static class XExtensionExtension
    {
        /// <summary>
        /// Creates Task Pane from the enumeration definition
        /// </summary>
        /// <typeparam name="TControl">Type of control</typeparam>
        /// <typeparam name="TEnum">Enumeration defining the commands for Task Pane</typeparam>
        /// <param name="ext">Extension</param>
        /// <returns>Task Pane instance</returns>
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

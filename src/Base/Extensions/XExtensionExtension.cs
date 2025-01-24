//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
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
        private class XWorkUnitUserResult<TRes> : IXWorkUnitUserResult<TRes>
        {
            public TRes Result { get; }

            internal XWorkUnitUserResult(TRes result)
            {
                Result = result;
            }
        }

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

        /// <summary>
        /// Creates new popup window
        /// </summary>
        /// <typeparam name="TWindow">Type of window</typeparam>
        /// <param name="ext">Extension</param>
        /// <returns>Popup window</returns>
        public static IXPopupWindow<TWindow> CreatePopupWindow<TWindow>(this IXExtension ext)
            where TWindow : new() => ext.CreatePopupWindow<TWindow>(new TWindow());

        /// <summary>
        /// Creates and runs work unit
        /// </summary>
        /// <typeparam name="TRes"></typeparam>
        /// <param name="ext">Extension</param>
        /// <param name="operation">Operation to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Executed work unit</returns>
        public static IXWorkUnit CreateWorkUnit<TRes>(this IXExtension ext, Func<IXProgress, CancellationToken, TRes> operation,
            CancellationToken cancellationToken = default)
        {
            var workUnit = ext.PreCreateWorkUnit();
            
            workUnit.Operation = new WorkUnitOperationDelegate(
                (p, c) => new XWorkUnitUserResult<TRes>(operation.Invoke(p, c)));

            workUnit.Commit(cancellationToken);

            return workUnit;
        }
    }
}

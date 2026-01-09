//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Enums;

namespace Xarial.XCad.Extensions
{
    /// <summary>
    /// Collection of additional functions of <see cref="IXApplication"/>
    /// </summary>
    public static class XApplicationExtension
    {
        private class TooltipSpec : ITooltipSpec
        {
            public string Title { get; }
            public string Message { get; }
            public Point Position { get; }
            public TooltipArrowPosition_e ArrowPosition { get; }

            public string LinkName { get; }

            public string Link { get; }

            internal TooltipSpec(string title, string msg, Point pt, TooltipArrowPosition_e arrPos, string link, string linkName) 
            {
                Title = title;
                Message = msg;
                Position = pt;
                ArrowPosition = arrPos;
                LinkName = linkName;
                Link = link;
            }
        }

        /// <summary>
        /// Displays the tooltip
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="title">Tooltip title</param>
        /// <param name="msg">Tooltip content (message)</param>
        /// <param name="pt">Tooltip position</param>
        /// <param name="arrPos">Arrow position of the tooltip</param>
        /// <param name="link">Tooltip link</param>
        /// <param name="linkName">User-friendly link name</param>
        public static void ShowTooltip(this IXApplication app, string title, string msg, Point pt, TooltipArrowPosition_e arrPos, string link = "", string linkName = "") 
            => app.ShowTooltip(new TooltipSpec(title, msg, pt, arrPos, link, linkName));
    }
}

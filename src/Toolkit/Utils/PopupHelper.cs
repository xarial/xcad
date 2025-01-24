//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.UI.Enums;

namespace Xarial.XCad.Toolkit.Utils
{
    /// <summary>
    /// Utilities for <see cref="UI.IXPopupWindow{TWindow}>"/>
    /// </summary>
    public static class PopupHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        /// <summary>
        /// Returns top-left location of the popup
        /// </summary>
        /// <param name="parentWnd">Parent window</param>
        /// <param name="dock"></param>
        /// <param name="scaleDpi">True to scale according to the screen resolution</param>
        /// <param name="width">Width of the popup</param>
        /// <param name="height">Height of the popup</param>
        /// <param name="padding">Padding of the popup</param>
        /// <returns>Top-left location</returns>
        public static Point CalculateLocation(IntPtr parentWnd, PopupDock_e dock, bool scaleDpi, double width, double height, Thickness padding)
        {
            GetWindowRect(parentWnd, out var rect);

            double scaleX;
            double scaleY;

            if (scaleDpi)
            {
                GetDpiScale(out scaleX, out scaleY);
            }
            else 
            {
                scaleX = 1;
                scaleY = 1;
            }

            var left = rect.Left / scaleX;
            var top = rect.Top / scaleY;

            var wndWidth = (rect.Right - rect.Left) / scaleX;
            var wndHeight = (rect.Bottom - rect.Top) / scaleY;

            switch (dock)
            {
                case PopupDock_e.Center:
                    return new Point(left + wndWidth / 2 - width / 2, top + wndHeight / 2 - height / 2, 0);

                case PopupDock_e.TopRight:
                    return new Point(left + wndWidth - width - padding.Right, top + padding.Top, 0);

                case PopupDock_e.TopLeft:
                    return new Point(left + padding.Left, top + padding.Top, 0);

                case PopupDock_e.BottomRight:
                    return new Point(left + wndWidth - width - padding.Right, top + wndHeight - height - padding.Bottom, 0);

                case PopupDock_e.BottomLeft:
                    return new Point(left + padding.Left, top + wndHeight - height - padding.Bottom, 0);

                default:
                    throw new NotSupportedException();
            }
        }

        private static void GetDpiScale(out double scaleX, out double scaleY)
        {
            using (var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
            {
                const int DPI = 96;

                scaleX = graphics.DpiX / DPI;
                scaleY = graphics.DpiY / DPI;
            }
        }
    }
}

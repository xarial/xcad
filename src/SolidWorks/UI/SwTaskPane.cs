//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.Commands.Toolkit.Structures;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI;
using Xarial.XCad.UI.TaskPane;
using Xarial.XCad.UI.TaskPane.Delegates;
using Xarial.XCad.Toolkit;
using Xarial.XCad.SolidWorks.UI.Toolkit;
using Xarial.XCad.Base;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Xarial.XCad.SolidWorks.UI
{
    public interface ISwTaskPane<TControl> : IXTaskPane<TControl>, IDisposable 
    {
        ITaskpaneView TaskPaneView { get; }
    }

    internal static class WinAPI
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

            public int X
            {
                get { return Left; }
                set { Right -= (Left - value); Left = value; }
            }

            public int Y
            {
                get { return Top; }
                set { Bottom -= (Top - value); Top = value; }
            }

            public int Height
            {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }

            public int Width
            {
                get { return Right - Left; }
                set { Right = value + Left; }
            }

            public System.Drawing.Point Location
            {
                get { return new System.Drawing.Point(Left, Top); }
                set { X = value.X; Y = value.Y; }
            }

            public System.Drawing.Size Size
            {
                get { return new System.Drawing.Size(Width, Height); }
                set { Width = value.Width; Height = value.Height; }
            }

            public static implicit operator System.Drawing.Rectangle(RECT r)
            {
                return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
            }

            public static implicit operator RECT(System.Drawing.Rectangle r)
            {
                return new RECT(r);
            }

            public static bool operator ==(RECT r1, RECT r2)
            {
                return r1.Equals(r2);
            }

            public static bool operator !=(RECT r1, RECT r2)
            {
                return !r1.Equals(r2);
            }

            public bool Equals(RECT r)
            {
                return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
            }

            public override bool Equals(object obj)
            {
                if (obj is RECT)
                    return Equals((RECT)obj);
                else if (obj is System.Drawing.Rectangle)
                    return Equals(new RECT((System.Drawing.Rectangle)obj));
                return false;
            }

            public override int GetHashCode()
            {
                return ((System.Drawing.Rectangle)this).GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
            }
        }

        [Flags()]
        internal enum RedrawWindowFlags : uint
        {
            /// <summary>
            /// Invalidates the rectangle or region that you specify in lprcUpdate or hrgnUpdate.
            /// You can set only one of these parameters to a non-NULL value. If both are NULL, RDW_INVALIDATE invalidates the entire window.
            /// </summary>
            Invalidate = 0x1,

            /// <summary>Causes the OS to post a WM_PAINT message to the window regardless of whether a portion of the window is invalid.</summary>
            InternalPaint = 0x2,

            /// <summary>
            /// Causes the window to receive a WM_ERASEBKGND message when the window is repainted.
            /// Specify this value in combination with the RDW_INVALIDATE value; otherwise, RDW_ERASE has no effect.
            /// </summary>
            Erase = 0x4,

            /// <summary>
            /// Validates the rectangle or region that you specify in lprcUpdate or hrgnUpdate.
            /// You can set only one of these parameters to a non-NULL value. If both are NULL, RDW_VALIDATE validates the entire window.
            /// This value does not affect internal WM_PAINT messages.
            /// </summary>
            Validate = 0x8,

            NoInternalPaint = 0x10,

            /// <summary>Suppresses any pending WM_ERASEBKGND messages.</summary>
            NoErase = 0x20,

            /// <summary>Excludes child windows, if any, from the repainting operation.</summary>
            NoChildren = 0x40,

            /// <summary>Includes child windows, if any, in the repainting operation.</summary>
            AllChildren = 0x80,

            /// <summary>Causes the affected windows, which you specify by setting the RDW_ALLCHILDREN and RDW_NOCHILDREN values, to receive WM_ERASEBKGND and WM_PAINT messages before the RedrawWindow returns, if necessary.</summary>
            UpdateNow = 0x100,

            /// <summary>
            /// Causes the affected windows, which you specify by setting the RDW_ALLCHILDREN and RDW_NOCHILDREN values, to receive WM_ERASEBKGND messages before RedrawWindow returns, if necessary.
            /// The affected windows receive WM_PAINT messages at the ordinary time.
            /// </summary>
            EraseNow = 0x200,

            Frame = 0x400,

            NoFrame = 0x800
        }

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        internal static extern bool UpdateWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern bool RedrawWindow(IntPtr hWnd, [In] ref RECT lprcUpdate, IntPtr hrgnUpdate, RedrawWindowFlags flags);

        private const int WmPaint = 0x000F;

        [DllImport("User32.dll")]
        public static extern Int64 SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public static void ForcePaint(IntPtr hWnd)
        {
            SendMessage(hWnd, WmPaint, IntPtr.Zero, IntPtr.Zero);
        }
    }

    internal class SwTaskPane<TControl> : ISwTaskPane<TControl>, IAutoDisposable
    {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event Action<IAutoDisposable> Disposed;

        public event TaskPaneButtonClickDelegate ButtonClick;
        public event ControlCreatedDelegate<TControl> ControlCreated;
        public event PanelActivatedDelegate<TControl> Activated;

        private readonly TaskPaneSpec m_Spec;

        private bool m_IsDisposed;

        public bool IsActive 
        {
            get => throw new NotImplementedException();
            set 
            {
                if (value)
                {
                    TaskPaneView.ShowView();
                }
                else 
                {
                    TaskPaneView.HideView();
                }
            }
        }

        public ITaskpaneView TaskPaneView { get; }
        public TControl Control { get; }

        public bool IsControlCreated => true;

        private WpfControlKeystrokePropagator m_KeystrokePropagator;

        private readonly TaskPaneTabCreator<TControl> m_Creator;
        private readonly IXLogger m_Logger;

        private readonly System.Windows.Forms.Control m_WinCtrl;

        internal SwTaskPane(TaskPaneTabCreator<TControl> creator, IXLogger logger)
        {
            m_Creator = creator;

            TControl ctrl;
            TaskPaneView = m_Creator.CreateControl(typeof(TControl), out ctrl, out m_WinCtrl);
            Control = ctrl;
            
            m_Logger = logger;

            if (ctrl is FrameworkElement)
            {
                m_KeystrokePropagator = new WpfControlKeystrokePropagator(ctrl as FrameworkElement);
            }

            m_Spec = m_Creator.Spec;

            (TaskPaneView as TaskpaneView).TaskPaneDestroyNotify += OnTaskPaneDestroyNotify;

            (TaskPaneView as TaskpaneView).TaskPaneActivateNotify += OnTaskPaneViewActivate;

            if (m_Spec.Buttons?.Any() == true)
            {
                (TaskPaneView as TaskpaneView).TaskPaneToolbarButtonClicked += OnTaskPaneToolbarButtonClicked;
            }
            
            m_IsDisposed = false;
            ControlCreated?.Invoke(Control);
        }

        private int OnTaskPaneViewActivate()
        {
            var hWnd = m_WinCtrl.Handle;

            WinAPI.ForcePaint(hWnd);

            Activated?.Invoke(this);

            return 0;
        }

        private int OnTaskPaneToolbarButtonClicked(int buttonIndex)
        {
            m_Logger.Log($"Task pane button clicked: {buttonIndex}", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);

            if (m_Spec.Buttons?.Length > buttonIndex)
            {
                ButtonClick?.Invoke(m_Spec.Buttons[buttonIndex]);
            }
            else
            {
                m_Logger.Log($"Invalid task pane button id is clicked: {buttonIndex}", XCad.Base.Enums.LoggerMessageSeverity_e.Error);
                Debug.Assert(false, "Invalid command id");
            }

            return HResult.S_OK;
        }

        private int OnTaskPaneDestroyNotify()
        {
            m_Logger.Log("Destroying task pane", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);

            Dispose();
            return HResult.S_OK;
        }

        public void Dispose()
            => Close();

        public void Close()
        {
            if (!m_IsDisposed)
            {
                m_IsDisposed = true;

                m_KeystrokePropagator?.Dispose();

                (TaskPaneView as TaskpaneView).TaskPaneActivateNotify -= OnTaskPaneViewActivate;
                (TaskPaneView as TaskpaneView).TaskPaneDestroyNotify -= OnTaskPaneDestroyNotify;
                (TaskPaneView as TaskpaneView).TaskPaneToolbarButtonClicked -= OnTaskPaneToolbarButtonClicked;

                try
                {
                    if (Control is IDisposable)
                    {
                        (Control as IDisposable).Dispose();
                    }
                }
                finally 
                {
                    if (!TaskPaneView.DeleteView())
                    {
                        throw new InvalidOperationException("Failed to remove TaskPane");
                    }
                }

                Disposed?.Invoke(this);
            }
        }
    }
}

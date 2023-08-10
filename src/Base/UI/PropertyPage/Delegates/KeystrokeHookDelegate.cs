using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Delegates
{

    /// <summary>
    /// Delegate for the <see cref="IXPropertyPage.KeystrokeHook"/> event
    /// </summary>
    /// <param name="ctrl">Control that has focus when keystroke was made</param>
    /// <param name="msg">Message identifier</param>
    /// <param name="wParam">wParam argument (keystroke)</param>
    /// <param name="lParam">lParam argument (bitmaps)</param>
    /// <param name="handled">True to indicate that keystroke has been handled and should not continue to process</param>
    public delegate void KeystrokeHookDelegate(IControl ctrl, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);
}

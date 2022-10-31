using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.UI.PropertyPage.Enums
{
    /// <summary>
    /// Automatic bitmap effects which can be applied to UI controls
    /// </summary>
    [Flags]
    public enum BitmapEffect_e
    {
        /// <summary>
        /// No effect
        /// </summary>
        None = 0,

        /// <summary>
        /// Grayscale bitmap
        /// </summary>
        Grayscale = 1,

        /// <summary>
        /// Semi-transparent bitmap
        /// </summary>
        Transparent = 2
    }
}

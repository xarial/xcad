//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xarial.XCad.Geometry.Structures
{
    /// <summary>
    /// Represents thickness
    /// </summary>
    /// <remarks>Usually used to define margin and padding</remarks>
    [DebuggerDisplay("{" + nameof(Left) + "} {" + nameof(Right) + "} {" + nameof(Top) + "} {" + nameof(Bottom) + "}")]
    public class Thickness
    {
        /// <summary>
        /// Left width
        /// </summary>
        public double Left { get; }

        /// <summary>
        /// Right width
        /// </summary>
        public double Right { get; }

        /// <summary>
        /// Toip width
        /// </summary>
        public double Top { get; }

        /// <summary>
        /// Bottom width
        /// </summary>
        public double Bottom { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Thickness(double left, double right, double top, double bottom) 
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        /// <summary>
        /// Constructor for universal thickness
        /// </summary>
        /// <param name="width">Width</param>
        public Thickness(double width) : this(width, width, width, width)
        {
        }
    }
}

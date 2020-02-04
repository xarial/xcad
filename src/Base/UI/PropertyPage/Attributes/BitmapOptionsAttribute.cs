//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Drawing;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Additional options for bitmap control
    /// </summary>
    /// <remarks>Applied to property of type <see cref="Image"/></remarks>
    public class BitmapOptionsAttribute : Attribute, IAttribute
    {
        public Size Size { get; private set; }

        /// <summary>
        /// Constructor for bitmap options
        /// </summary>
        /// <param name="width">Width of the bitmap</param>
        /// <param name="height">Height of the bitmap</param>
        public BitmapOptionsAttribute(int width, int height)
        {
            Size = new Size(width, height);
        }
    }
}
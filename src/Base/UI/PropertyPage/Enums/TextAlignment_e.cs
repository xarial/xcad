//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.UI.PropertyPage.Enums
{
    /// <summary>
    /// Alignment option for the text
    /// </summary>
    [Flags]
    public enum TextAlignment_e
    {
        /// <summary>
        /// Default alignment
        /// </summary>
        Default = 0,

        /// <summary>
        /// Left alignment
        /// </summary>
        Left = 1,

        /// <summary>
        /// Center alignment
        /// </summary>
        Center = 2,

        /// <summary>
        /// Right alignment
        /// </summary>
        Right = 4
    }
}
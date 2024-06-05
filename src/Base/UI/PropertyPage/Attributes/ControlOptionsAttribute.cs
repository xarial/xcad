﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Drawing;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <inheritdoc/>
    public class ControlOptionsAttribute : Attribute, IControlOptionsAttribute
    {
        /// <inheritdoc/>
        public AddControlOptions_e Options { get; }

        /// <inheritdoc/>
        public ControlLeftAlign_e Align { get; }

        /// <inheritdoc/>
        public KnownColor BackgroundColor { get; }

        /// <inheritdoc/>
        public KnownColor TextColor { get; }

        /// <inheritdoc/>
        public short Left { get; }

        /// <inheritdoc/>
        public short Top { get; }

        /// <inheritdoc/>
        public short Width { get; }

        /// <inheritdoc/>
        public short Height { get; }

        /// <inheritdoc/>
        public ControlOnResizeOptions_e ResizeOptions { get; }

        /// <summary>
        /// Constructor allowing to specify control common parameters
        /// </summary>
        /// <param name="opts">Generic control options</param>
        /// <param name="align">Control alignment options</param>
        /// <param name="backgroundColor">Background color of control. Use 0 for default color</param>
        /// <param name="textColor">Color of the text on the control. Use 0 for default color</param>
        /// <param name="left">Left alignment of the control. Use -1 for default alignment</param>
        /// <param name="top">Top alignment of the control. Use -1 to align the control under the previous control</param>
        /// <param name="width">Width of the control. Use -1 for auto width</param>
        /// <param name="height">Height of the control in property manager page dialog box units. Use -1 for the auto height</param>
        /// <param name="resizeOptions">Options to resize</param>
        public ControlOptionsAttribute(
            AddControlOptions_e opts = AddControlOptions_e.Enabled | AddControlOptions_e.Visible,
            ControlLeftAlign_e align = ControlLeftAlign_e.LeftEdge,
            KnownColor backgroundColor = 0, KnownColor textColor = 0, short left = -1, short top = -1, short width = -1, short height = -1,
            ControlOnResizeOptions_e resizeOptions = 0)
        {
            Options = opts;
            Align = align;
            BackgroundColor = backgroundColor;
            TextColor = textColor;
            Left = left;
            Top = top;
            Width = width;
            Height = height;
            ResizeOptions = resizeOptions;
        }
    }
}
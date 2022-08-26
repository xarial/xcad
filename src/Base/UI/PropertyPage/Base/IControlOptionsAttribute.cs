//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.UI.PropertyPage.Base
{
    /// <summary>
    /// Generic options for all controls
    /// </summary>
    public interface IControlOptionsAttribute : IAttribute
    {
        /// <summary>
        /// Control options
        /// </summary>
        AddControlOptions_e Options { get; }

        /// <summary>
        /// Alignment of the control
        /// </summary>
        ControlLeftAlign_e Align { get; }

        /// <summary>
        /// Background color of the control
        /// </summary>
        KnownColor BackgroundColor { get; }

        /// <summary>
        /// Foreground color of the control
        /// </summary>
        KnownColor TextColor { get; }

        /// <summary>
        /// Left offset of the control
        /// </summary>
        short Left { get; }

        /// <summary>
        /// Top offset of the control
        /// </summary>
        short Top { get; }

        /// <summary>
        /// Width of the control
        /// </summary>
        short Width { get; }

        /// <summary>
        /// Height of the control
        /// </summary>
        short Height { get; }

        /// <summary>
        /// Resize options
        /// </summary>
        ControlOnResizeOptions_e ResizeOptions { get; }
    }
}

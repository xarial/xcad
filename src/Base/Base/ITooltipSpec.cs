//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xarial.XCad.Enums;

namespace Xarial.XCad.Base
{
    /// <summary>
    /// Defines the specification of the tooltip used in the <see cref="IXApplication.ShowTooltip(ITooltipSpec)"/>
    /// </summary>
    public interface ITooltipSpec
    {
        /// <summary>
        /// Title of tooltip
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Message to show in tooltip
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Position of the tooltip
        /// </summary>
        Point Position { get; }

        /// <summary>
        /// Position of tooltip arrow
        /// </summary>
        TooltipArrowPosition_e ArrowPosition { get; }
    }
}

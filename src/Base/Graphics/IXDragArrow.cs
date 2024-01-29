//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Graphics
{
    /// <summary>
    /// Delegate of <see cref="IXDragArrow.Flipped"/> event
    /// </summary>
    /// <param name="sender">Drag arrow manipulator</param>
    /// <param name="direction">New direction</param>
    public delegate void DragArrowFlippedDelegate(IXDragArrow sender, Vector direction);

    /// <summary>
    /// Delegate of <see cref="IXDragArrow.Selected"/> event
    /// </summary>
    /// <param name="sender">Drag arrow manipulator</param>
    public delegate void DragArrowSelectedDelegate(IXDragArrow sender);

    /// <summary>
    /// Drag arrow manipulator
    /// </summary>
    public interface IXDragArrow : IXObject, IXTransaction, IDisposable
    {
        /// <summary>
        /// Event is raised when the direction is flipped
        /// </summary>
        event DragArrowFlippedDelegate Flipped;

        /// <summary>
        /// Event is raised when drag arrow manipulator is selected
        /// </summary>
        event DragArrowSelectedDelegate Selected;

        /// <summary>
        /// True if the direction can be flipped
        /// </summary>
        bool CanFlip { get; set; }

        /// <summary>
        /// Length of the manipulator
        /// </summary>
        double Length { get; set; }

        /// <summary>
        /// Direction of the arrow
        /// </summary>
        Vector Direction { get; set; }

        /// <summary>
        /// Origin of the arrow
        /// </summary>
        Point Origin { get; set; }

        /// <summary>
        /// Controls the visibility of this triad
        /// </summary>
        bool Visible { get; set; }
    }
}

//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
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
    /// Elements of triad manipulator
    /// </summary>
    [Flags]
    public enum TriadElements_e 
    {
        /// <summary>
        /// Origin
        /// </summary>
        Origin = 1,

        /// <summary>
        /// X-Axis
        /// </summary>
        AxisX = 2,

        /// <summary>
        /// Y-Axis
        /// </summary>
        AxisY = 4,

        /// <summary>
        /// Z-Axis
        /// </summary>
        AxisZ = 8,

        /// <summary>
        /// Rotatioin ring around X axis
        /// </summary>
        RingX = 16,

        /// <summary>
        /// Rotatioin ring around Y axis
        /// </summary>
        RingY = 32,

        /// <summary>
        /// Rotatioin ring around Z axis
        /// </summary>
        RingZ = 64,

        /// <summary>
        /// Shows all elements of triad
        /// </summary>
        All = Origin | AxisX | AxisY | AxisZ | RingX | RingY | RingZ,
    }

    /// <summary>
    /// Delegate for <see cref="IXTriad.Selected"/> event
    /// </summary>
    /// <param name="sender">Triad</param>
    /// <param name="element">Element being selected</param>
    public delegate void TriadSelectedDelegate(IXTriad sender, TriadElements_e element);

    /// <summary>
    /// Delegate of <see cref="IXTriad.Manipulated"/>
    /// </summary>
    /// <param name="triad">Triad</param>
    /// <param name="transform">Transformation of the triad</param>
    public delegate void TriadManipulatedDelegate(IXTriad triad, TransformMatrix transform);

    /// <summary>
    /// Represents the triad manipulator
    /// </summary>
    public interface IXTriad : IXObject, IXTransaction, IDisposable
    {
        /// <summary>
        /// Raised when the element of the triad is selected
        /// </summary>
        event TriadSelectedDelegate Selected;

        /// <summary>
        /// Raised when the triad is manipulated
        /// </summary>
        event TriadManipulatedDelegate Manipulated;

        /// <summary>
        /// Elements of this triad
        /// </summary>
        TriadElements_e Elements { get; set; }

        /// <summary>
        /// Transformation of this triad
        /// </summary>
        TransformMatrix Transform { get; set; }

        /// <summary>
        /// Entities attached to this triad
        /// </summary>
        IXSelObject[] Entities { get; set; }

        /// <summary>
        /// Controls the visibility of this triad
        /// </summary>
        bool Visible { get; set; }
    }
}

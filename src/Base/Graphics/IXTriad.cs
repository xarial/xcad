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
        /// Shows all elements of triad
        /// </summary>
        All = Origin | AxisX | AxisY | AxisZ,
    }

    /// <summary>
    /// Represents the triad manipulator
    /// </summary>
    public interface IXTriad : IXObject, IXTransaction, IDisposable
    {
        /// <summary>
        /// Elements of this triad
        /// </summary>
        TriadElements_e Elements { get; set; }

        /// <summary>
        /// Transformation of this triad
        /// </summary>
        TransformMatrix Transform { get; set; }

        /// <summary>
        /// Controls the visibility of this triad
        /// </summary>
        bool Visible { get; set; }
    }
}

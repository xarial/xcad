//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Documents.Enums
{
    /// <summary>
    /// Direction of the view projection of <see cref="IXProjectedDrawingView"/>
    /// </summary>
    public enum ProjectedViewDirection_e
    {
        /// <summary>
        /// Left
        /// </summary>
        Left,

        /// <summary>
        /// Top
        /// </summary>
        Top,

        /// <summary>
        /// Right
        /// </summary>
        Right,

        /// <summary>
        /// Bottom
        /// </summary>
        Bottom,

        /// <summary>
        /// Isometric Top Left
        /// </summary>
        IsoTopLeft,

        /// <summary>
        /// Isometric Top Right
        /// </summary>
        IsoTopRight,

        /// <summary>
        /// Isometric Bottom Left
        /// </summary>
        IsoBottomLeft,

        /// <summary>
        /// Isolmetric Bottom Right
        /// </summary>
        IsoBottomRight
    }

    /// <summary>
    /// Extension of <see cref="ProjectedViewDirection_e"/>
    /// </summary>
    public static class ProjectedViewDirectionExtension
    {
        /// <summary>
        /// Converts projection direction to vector
        /// </summary>
        /// <param name="projection">Projection</param>
        /// <returns>Vector of projection</returns>
        /// <exception cref="NotSupportedException">Invalid projection</exception>
        public static Vector ToVector(this ProjectedViewDirection_e projection)
        {
            double dirX;
            double dirY;

            switch (projection)
            {
                case ProjectedViewDirection_e.Left:
                    dirX = -1;
                    dirY = 0;
                    break;

                case ProjectedViewDirection_e.Top:
                    dirX = 0;
                    dirY = 1;
                    break;

                case ProjectedViewDirection_e.Right:
                    dirX = 1;
                    dirY = 0;
                    break;

                case ProjectedViewDirection_e.Bottom:
                    dirX = 0;
                    dirY = -1;
                    break;

                case ProjectedViewDirection_e.IsoTopLeft:
                    dirX = -1;
                    dirY = 1;
                    break;

                case ProjectedViewDirection_e.IsoTopRight:
                    dirX = 1;
                    dirY = 1;
                    break;

                case ProjectedViewDirection_e.IsoBottomLeft:
                    dirX = -1;
                    dirY = -1;
                    break;

                case ProjectedViewDirection_e.IsoBottomRight:
                    dirX = 1;
                    dirY = -1;
                    break;

                default:
                    throw new NotSupportedException($"'{projection}' is not supported");
            }

            return new Vector(dirX, dirY, 0);
        }
    }
}

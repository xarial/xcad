//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry.Primitives;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Represents geometry builder interface
    /// </summary>
    public interface IX3DGeometryBuilder 
    {
        /// <summary>
        /// Creates an extrusion template
        /// </summary>
        /// <returns>Extrusion template</returns>
        IXExtrusion PreCreateExtrusion();

        /// <summary>
        /// Creates sweep template
        /// </summary>
        /// <returns>Sweep template</returns>
        IXSweep PreCreateSweep();

        /// <summary>
        /// Creates loft template
        /// </summary>
        /// <returns>Loft template</returns>
        IXLoft PreCreateLoft();

        /// <summary>
        /// Creates revolve template
        /// </summary>
        /// <returns>Revolve template</returns>
        IXRevolve PreCreateRevolve();

        /// <summary>
        /// Creates knit template
        /// </summary>
        /// <returns>Knit template</returns>
        IXKnit PreCreateKnit();
    }
}

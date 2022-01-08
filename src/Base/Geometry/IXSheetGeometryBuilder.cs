//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry.Primitives;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Provides methods to buld sheet geometry
    /// </summary>
    public interface IXSheetGeometryBuilder : IX3DGeometryBuilder
    {
        /// <summary>
        /// Creates new instance of planar sheet
        /// </summary>
        /// <returns>Planar sheet template</returns>
        IXPlanarSheet PreCreatePlanarSheet();
    }
}

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
    /// Provides methods to buld sheet geometry
    /// </summary>
    public interface IXSheetGeometryBuilder : IX3DGeometryBuilder
    {   
    }

    /// <summary>
    /// Additional methods for <see cref="IXSheetGeometryBuilder"/>
    /// </summary>
    public static class XSheetGeometryBuilderExtension 
    {
        /// <summary>
        /// Creates new instance of planar sheet
        /// </summary>
        /// <returns>Planar sheet template</returns>
        public static IXPlanarSheet PreCreatePlanarSheet(this IXSheetGeometryBuilder geomBuilder) => geomBuilder.PreCreate<IXPlanarSheet>();
    }
}

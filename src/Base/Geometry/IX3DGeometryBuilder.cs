//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Geometry.Primitives;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Represents geometry builder interface
    /// </summary>
    public interface IX3DGeometryBuilder : IXRepository<IXPrimitive>
    {
        
    }

    /// <summary>
    /// Additional methods for <see cref="IX3DGeometryBuilder"/>
    /// </summary>
    public static class X3DGeometryBuilderExtension 
    {
        /// <summary>
        /// Creates an extrusion template
        /// </summary>
        /// <returns>Extrusion template</returns>
        public static IXExtrusion PreCreateExtrusion(this IX3DGeometryBuilder geomBuilder) => geomBuilder.PreCreate<IXExtrusion>();

        /// <summary>
        /// Creates sweep template
        /// </summary>
        /// <returns>Sweep template</returns>
        public static IXSweep PreCreateSweep(this IX3DGeometryBuilder geomBuilder) => geomBuilder.PreCreate<IXSweep>();

        /// <summary>
        /// Creates loft template
        /// </summary>
        /// <returns>Loft template</returns>
        public static IXLoft PreCreateLoft(this IX3DGeometryBuilder geomBuilder) => geomBuilder.PreCreate<IXLoft>();

        /// <summary>
        /// Creates revolve template
        /// </summary>
        /// <returns>Revolve template</returns>
        public static IXRevolve PreCreateRevolve(this IX3DGeometryBuilder geomBuilder) => geomBuilder.PreCreate<IXRevolve>();

        /// <summary>
        /// Creates knit template
        /// </summary>
        /// <returns>Knit template</returns>
        public static IXKnit PreCreateKnit(this IX3DGeometryBuilder geomBuilder) => geomBuilder.PreCreate<IXKnit>();
    }
}

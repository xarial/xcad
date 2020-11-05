using Xarial.XCad.Geometry.Primitives;

namespace Xarial.XCad.Geometry
{
    public interface IXSheetGeometryBuilder : IX3DGeometryBuilder
    {
        IXPlanarSheet PreCreatePlanarSheet();
    }
}

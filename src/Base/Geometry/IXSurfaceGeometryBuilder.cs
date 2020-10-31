using Xarial.XCad.Geometry.Primitives;

namespace Xarial.XCad.Geometry
{
    public interface IXSurfaceGeometryBuilder : IX3DGeometryBuilder
    {
        IXPlane PreCreatePlane();
    }
}

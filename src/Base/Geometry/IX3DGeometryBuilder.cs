using Xarial.XCad.Geometry.Primitives;

namespace Xarial.XCad.Geometry
{
    public interface IX3DGeometryBuilder 
    {
        IXExtrusion PreCreateExtrusion();
        IXSweep PreCreateSweep();
        IXLoft PreCreateLoft();
        IXRevolve PreCreateRevolve();
    }
}

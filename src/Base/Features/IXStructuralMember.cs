using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Sketch;

namespace Xarial.XCad.Features
{
    public interface IXSructuralMemberGroupRepository : IXRepository<IXStructuralMemberGroup>
    {
    }

    public interface IXStructuralMemberGroup : IXTransaction
    {
        string Name { get; }
        IXSructuralMemberPieceRepository Pieces { get; }
    }

    public interface IXSructuralMemberPieceRepository : IXRepository<IXStructuralMemberPiece>
    {
    }

    public interface IXStructuralMemberPiece : IXTransaction
    {
        IXSolidBody Body { get; }
        IXSketchSegment[] Segments { get; }
        Plane ProfilePlane { get; }
    }

    /// <summary>
    /// Represents the weldment structural member feature
    /// </summary>
    public interface IXStructuralMember : IXFeature
    {
        IXSketch2D Profile { get; }
        IXSructuralMemberGroupRepository Groups { get; }
    }
}

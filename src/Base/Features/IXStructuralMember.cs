//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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
    /// <summary>
    /// Repository for structural member groups
    /// </summary>
    public interface IXSructuralMemberGroupRepository : IXRepository<IXStructuralMemberGroup>
    {
    }

    /// <summary>
    /// Represents the group in the <see cref="IXStructuralMember"/>
    /// </summary>
    public interface IXStructuralMemberGroup : IXTransaction
    {
        /// <summary>
        /// Name of the structural member group
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Structural member pieces belong to this group
        /// </summary>
        IXSructuralMemberPieceRepository Pieces { get; }
    }

    /// <summary>
    /// Repository of structural member pieces
    /// </summary>
    public interface IXSructuralMemberPieceRepository : IXRepository<IXStructuralMemberPiece>
    {
    }

    /// <summary>
    /// Piece in the structural member group
    /// </summary>
    public interface IXStructuralMemberPiece : IXTransaction
    {
        /// <summary>
        /// Body of the piece
        /// </summary>
        IXSolidBody Body { get; }

        /// <summary>
        /// Sketch segments forming the piece
        /// </summary>
        IXSketchSegment[] Segments { get; }

        /// <summary>
        /// Plane of the profile
        /// </summary>
        Plane ProfilePlane { get; }
    }

    /// <summary>
    /// Represents the weldment structural member feature
    /// </summary>
    public interface IXStructuralMember : IXFeature
    {
        /// <summary>
        /// Profile of this structural member
        /// </summary>
        IXSketch2D Profile { get; }

        /// <summary>
        /// Groups belong to this structural member
        /// </summary>
        IXSructuralMemberGroupRepository Groups { get; }
    }
}

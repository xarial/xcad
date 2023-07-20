//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Numerics;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Sketch
{
    /// <summary>
    /// Represents the collection of entities (lines, arcs, points) in the sketch
    /// </summary>
    public interface IXSketchEntityRepository : IXWireGeometryBuilder
    {
    }

    /// <summary>
    /// Additional methods of <see cref="IXSketchEntityRepository"/>
    /// </summary>
    public static class XSketchEntityRepositoryExtension 
    {
    }
}
//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Surfaces;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Provides access to geometry buidling functions
    /// </summary>
    public interface IXGeometryBuilder
    {
        /// <summary>
        /// Provides an access to wire geometry builder functions
        /// </summary>
        IXWireGeometryBuilder WireBuilder { get; }

        /// <summary>
        /// Provides an access to sheet geometry builder functions
        /// </summary>
        IXSheetGeometryBuilder SheetBuilder { get; }

        /// <summary>
        /// Provides an access to solid geometry builder functions
        /// </summary>
        IXSolidGeometryBuilder SolidBuilder { get; }

        /// <summary>
        /// Creates region from the specified list of segments
        /// </summary>
        /// <param name="segments">Segments</param>
        /// <returns>Created region</returns>
        IXRegion CreateRegionFromSegments(params IXSegment[] segments);
    }

    /// <summary>
    /// Geometry builder for building in-memory geometry objects
    /// </summary>
    public interface IXMemoryGeometryBuilder : IXGeometryBuilder 
    {
        /// <summary>
        /// Deserializes memory body from the stream
        /// </summary>
        /// <param name="stream">Stream to deserialize body from</param>
        /// <returns>Deserialized body</returns>
        IXBody DeserializeBody(Stream stream);

        /// <summary>
        /// Serializes body into the stream
        /// </summary>
        /// <param name="body">Body to store</param>
        /// <param name="stream">Stream to store to</param>
        void SerializeBody(IXBody body, Stream stream);
    }
}

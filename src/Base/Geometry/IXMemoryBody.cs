﻿//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Represents the memory (temp) body object
    /// </summary>
    public interface IXMemoryBody : IXBody, IDisposable
    {
        /// <summary>
        /// DIsplays the preview of the memory body
        /// </summary>
        /// <param name="doc">Document where preview should be displayed</param>
        /// <param name="color">Color of the body</param>
        void Preview(IXDocument3D doc, Color color);
    }

    /// <summary>
    /// Represents the memory (temp) sheet (surface) body
    /// </summary>
    public interface IXMemorySheetBody : IXMemoryBody, IXSheetBody
    {
    }

    /// <summary>
    /// Subtype of <see cref="IXMemorySheetBody"/> which is planar
    /// </summary>
    public interface IXMemoryPlanarSheetBody : IXMemorySheetBody, IXPlanarSheetBody
    {
    }

    /// <summary>
    /// Represents the memory (temp) solid body geometry
    /// </summary>
    public interface IXMemorySolidBody : IXMemoryBody, IXSolidBody
    {
    }

    /// <summary>
    /// Represents the the memory (temp) wire body
    /// </summary>
    public interface IXMemoryWireBody : IXMemoryBody, IXWireBody
    {
    }
}
//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents the part document
    /// </summary>
    public interface IXPart : IXDocument3D
    {
        /// <summary>
        /// Fired when cut list is regenerated
        /// </summary>
        event CutListRebuildDelegate CutListRebuild;

        /// <summary>
        /// Bodies in this part document
        /// </summary>
        IXBodyRepository Bodies { get; }
    }
}
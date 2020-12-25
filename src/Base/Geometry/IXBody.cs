//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Geometry.Primitives;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Represents the body object
    /// </summary>
    public interface IXBody : IXSelObject, IXColorizable, IXTransaction
    {
        /// <summary>
        /// Name of the body
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Is body visible
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// Boolean add operation on body
        /// </summary>
        /// <param name="other">Other body</param>
        /// <returns>Resulting body</returns>
        IXBody Add(IXBody other);

        /// <summary>
        /// Boolean substract operation
        /// </summary>
        /// <param name="other">Body to substract</param>
        /// <returns>Resulting bodies</returns>
        IXBody[] Substract(IXBody other);

        /// <summary>
        /// Boolean common operation
        /// </summary>
        /// <param name="other">Body to get common with</param>
        /// <returns>Resulting body</returns>
        IXBody[] Common(IXBody other);
    }

    /// <summary>
    /// Represents sheet (surface) body
    /// </summary>
    public interface IXSheetBody : IXBody
    {
    }

    /// <summary>
    /// Subtype of <see cref="IXSheetBody"/> which is planar
    /// </summary>
    public interface IXPlanarSheetBody : IXSheetBody, IXRegion 
    {
    }

    /// <summary>
    /// Represents solid body geometry
    /// </summary>
    public interface IXSolidBody : IXBody 
    {
    }
}
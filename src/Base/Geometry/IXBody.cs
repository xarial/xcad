//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Represents the body object
    /// </summary>
    public interface IXBody : IXSelObject, IXColorizable
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
}
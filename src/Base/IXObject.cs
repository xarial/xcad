//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad
{
    /// <summary>
    /// Wrapper inteface over the specific object
    /// </summary>
    public interface IXObject
    {
        /// <summary>
        /// Method to compare the wrappers
        /// </summary>
        /// <param name="other">Other object to compare</param>
        /// <returns>True if underlying objects are same, False if not</returns>
        bool IsSame(IXObject other);
    }
}
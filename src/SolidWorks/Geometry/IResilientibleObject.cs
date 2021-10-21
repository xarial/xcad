//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface IResilientibleObject 
    {
        /// <summary>
        /// Is object resilient to regeneration
        /// </summary>
        bool IsResilient { get; }

        /// <summary>
        /// Converts this object to resilient object
        /// </summary>
        /// <returns>Resilient object</returns>
        ISwObject CreateResilient();
    }

    /// <summary>
    /// Indicates that this object can be resilient to the regeneration operations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IResilientibleObject<T> : IResilientibleObject
        where T : ISwObject
    {
        /// <summary>
        /// Specific implementation of resilient object
        /// </summary>
        /// <returns>Resilient object</returns>
        new T CreateResilient();
    }
}
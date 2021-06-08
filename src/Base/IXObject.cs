//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.IO;

namespace Xarial.XCad
{
    /// <summary>
    /// Wrapper inteface over the specific object
    /// </summary>
    public interface IXObject : IEquatable<IXObject>
    {
        /// <summary>
        /// Identifies if current object is valid
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Saves this object into a stream
        /// </summary>
        /// <param name="stream">Target stream</param>
        void Serialize(Stream stream);
    }
}
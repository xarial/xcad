//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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
        /// Saves this object into a stream
        /// </summary>
        /// <param name="stream">Target stream</param>
        void Serialize(Stream stream);
    }
}
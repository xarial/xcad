//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.IO;
using Xarial.XCad.Data;

namespace Xarial.XCad
{
    /// <summary>
    /// Wrapper interface over the specific object
    /// </summary>
    public interface IXObject : IEquatable<IXObject>
    {
        /// <summary>
        /// Identifies if current object is valid
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Provides an ability to store temp tags in this session
        /// </summary>
        ITagsManager Tags { get; }

        /// <summary>
        /// Saves this object into a stream
        /// </summary>
        /// <param name="stream">Target stream</param>
        void Serialize(Stream stream);
    }
}

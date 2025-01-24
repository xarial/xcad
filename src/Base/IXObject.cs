//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.IO;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;

namespace Xarial.XCad
{
    /// <summary>
    /// Wrapper interface over the specific object
    /// </summary>
    public interface IXObject : IXTransaction, IEquatable<IXObject>
    {
        /// <summary>
        /// Application which owns this object
        /// </summary>
        IXApplication OwnerApplication { get; }

        /// <summary>
        /// Document which owns this object
        /// </summary>
        /// <remarks>This can be null for the application level objects</remarks>
        IXDocument OwnerDocument { get; }

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

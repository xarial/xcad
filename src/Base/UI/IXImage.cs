//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.UI
{
    /// <summary>
    /// Represents the image
    /// </summary>
    /// <remarks>This is usually used for icons</remarks>
    public interface IXImage
    {
        /// <summary>
        /// Byte data of this image
        /// </summary>
        byte[] Buffer { get; }
    }

    /// <summary>
    /// Represents base image
    /// </summary>
    public class BaseImage : IXImage
    {
        /// <inheritdoc/>
        public byte[] Buffer { get; }

        /// <summary>
        /// Base image constructor
        /// </summary>
        /// <param name="buffer">Image buffer</param>
        public BaseImage(byte[] buffer) 
        {
            Buffer = buffer;
        }
    }
}

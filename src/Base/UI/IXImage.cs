//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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

    internal class XImage : IXImage
    {
        public byte[] Buffer { get; }

        internal XImage(byte[] buffer) 
        {
            Buffer = buffer;
        }
    }
}

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
    public interface IXImage
    {
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

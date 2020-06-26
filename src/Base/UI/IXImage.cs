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

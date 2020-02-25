//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace Xarial.XCad.Toolkit.Data
{
    public class ComStream : Stream
    {
        private readonly bool m_Commit;

        private bool m_IsWritable;

        public IStream Stream { get; private set; }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => m_IsWritable;

        public override long Length
        {
            get
            {
                const int STATSFLAG_NONAME = 1;

                STATSTG statstg;

                Stream.Stat(out statstg, STATSFLAG_NONAME);

                return statstg.cbSize;
            }
        }

        public override long Position
        {
            get
            {
                return Seek(0, SeekOrigin.Current);
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public ComStream(IStream comStream, bool writable, bool commit = true)
        {
            if (comStream == null)
            {
                throw new ArgumentNullException(nameof(comStream));
            }

            Stream = comStream;
            m_Commit = commit;
            m_IsWritable = writable;
        }

        public override void Flush()
        {
            if (m_Commit)
            {
                const int STGC_DEFAULT = 0;

                Stream.Commit(STGC_DEFAULT);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;
            object boxBytesRead = bytesRead; //must be boxed otherwise - will fail
            var hObject = default(System.Runtime.InteropServices.GCHandle);

            try
            {
                hObject = System.Runtime.InteropServices.GCHandle.Alloc(boxBytesRead,
                    System.Runtime.InteropServices.GCHandleType.Pinned);

                var pBytesRead = hObject.AddrOfPinnedObject();

                if (offset != 0)
                {
                    var tmpBuffer = new byte[count];
                    Stream.Read(tmpBuffer, count, pBytesRead);
                    bytesRead = Convert.ToInt32(boxBytesRead);
                    Array.Copy(tmpBuffer, 0, buffer, offset, bytesRead);
                }
                else
                {
                    Stream.Read(buffer, count, pBytesRead);
                    bytesRead = Convert.ToInt32(boxBytesRead);
                }
            }
            finally
            {
                if (hObject.IsAllocated)
                {
                    hObject.Free();
                }
            }

            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long curPosition = 0;
            var boxCurPosition = curPosition; //must be boxed otherwise - will fail
            var hObject = default(System.Runtime.InteropServices.GCHandle);

            try
            {
                hObject = System.Runtime.InteropServices.GCHandle.Alloc(
                    boxCurPosition, System.Runtime.InteropServices.GCHandleType.Pinned);

                var pCurPosition = hObject.AddrOfPinnedObject();

                Stream.Seek(offset, (int)origin, pCurPosition);
                curPosition = Convert.ToInt64(boxCurPosition);
            }
            finally
            {
                if (hObject.IsAllocated)
                {
                    hObject.Free();
                }
            }

            return curPosition;
        }

        public override void SetLength(long value)
        {
            Stream.SetSize(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (offset != 0)
            {
                var bufferSize = buffer.Length - offset;
                var tmpBuffer = new byte[bufferSize];
                Array.Copy(buffer, offset, tmpBuffer, 0, bufferSize);
                Stream.Write(tmpBuffer, bufferSize, IntPtr.Zero);
            }
            else
            {
                Stream.Write(buffer, count, IntPtr.Zero);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    m_IsWritable = false;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        ~ComStream()
        {
            Dispose(false);
        }
    }
}

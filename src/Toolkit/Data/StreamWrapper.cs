//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Xarial.XCad.Toolkit.Data
{
    public class StreamWrapper : IStream
    {
        private readonly Stream m_Stream;

        public StreamWrapper(Stream streamWrap)
        {
            m_Stream = streamWrap;
        }

        public void Clone(out IStream ppstm)
            => ppstm = new StreamWrapper(m_Stream);

        public void Commit(int grfCommitFlags)
            => m_Stream.Flush();
        
        public void Read(byte[] pv, int cb, IntPtr pcbRead)
            => System.Runtime.InteropServices.Marshal.WriteInt64(pcbRead, m_Stream.Read(pv, 0, cb));

        public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
        {
            const int STREAM_SEEK_SET = 0;
            const int STREAM_SEEK_CUR = 1;
            const int STREAM_SEEK_END = 2;

            long posMoveTo;

            System.Runtime.InteropServices.Marshal.WriteInt64(plibNewPosition, m_Stream.Position);

            switch (dwOrigin)
            {
                case STREAM_SEEK_SET:
                    {
                        posMoveTo = dlibMove;
                    }
                    break;
                case STREAM_SEEK_CUR:
                    {
                        posMoveTo = m_Stream.Position + dlibMove;
                    }
                    break;
                case STREAM_SEEK_END:
                    {
                        posMoveTo = m_Stream.Length + dlibMove;
                    }
                    break;
                default:
                    return;
            }

            if (posMoveTo >= 0 && posMoveTo < m_Stream.Length)
            {
                m_Stream.Position = posMoveTo;
                System.Runtime.InteropServices.Marshal.WriteInt64(plibNewPosition, m_Stream.Position);
            }
        }

        public void SetSize(long libNewSize)
            => m_Stream.SetLength(libNewSize);

        public void Stat(out STATSTG pstatstg, int grfStatFlag)
        {
            const int STATFLAG_NONAME = 0x0001;
            pstatstg = new STATSTG();
            pstatstg.cbSize = m_Stream.Length;

            if ((grfStatFlag & STATFLAG_NONAME) != 0)
            {
                return;
            }

            pstatstg.pwcsName = m_Stream.ToString();
        }

        public void Write(byte[] pv, int cb, IntPtr pcbWritten)
        {
            System.Runtime.InteropServices.Marshal.WriteInt64(pcbWritten, 0);
            m_Stream.Write(pv, 0, cb);
            System.Runtime.InteropServices.Marshal.WriteInt64(pcbWritten, cb);
        }

        #region NotSupported

        public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
            => throw new NotImplementedException();

        public void LockRegion(long libOffset, long cb, int dwLockType)
            => throw new NotImplementedException();

        public void Revert()
            => throw new NotImplementedException();

        public void UnlockRegion(long libOffset, long cb, int dwLockType)
            => throw new NotImplementedException();

        #endregion
    }
}

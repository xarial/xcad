//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Xarial.XCad.Data;

namespace Xarial.XCad.Toolkit.Data
{
    // #region WinAPI

    [Flags]
    internal enum STGM : int
    {
        READ = 0x0,
        WRITE = 0x1,
        READWRITE = 0x2,
        SHARE_DENY_NONE = 0x40,
        SHARE_DENY_READ = 0x30,
        SHARE_DENY_WRITE = 0x20,
        SHARE_EXCLUSIVE = 0x10,
        PRIORITY = 0x40000,
        CREATE = 0x1000,
        CONVERT = 0x20000,
        FAILIFTHERE = 0x0,
        DIRECT = 0x0,
        TRANSACTED = 0x10000,
        NOSCRATCH = 0x100000,
        NOSNAPSHOT = 0x200000,
        SIMPLE = 0x8000000,
        DIRECT_SWMR = 0x400000,
        DELETEONRELEASE = 0x4000000
    }

    internal enum STGTY : int
    {
        STORAGE = 1,
        STREAM = 2,
        LOCKBYTES = 3,
        PROPERTY = 4
    };

    [ComImport]
    [Guid("0000000d-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumSTATSTG
    {
        [PreserveSig]
        uint Next(uint celt,
        [MarshalAs(UnmanagedType.LPArray), Out]
        System.Runtime.InteropServices.ComTypes.STATSTG[] rgelt,
        out uint pceltFetched
        );

        void Skip(uint celt);

        void Reset();

        [return: MarshalAs(UnmanagedType.Interface)]
        IEnumSTATSTG Clone();
    }

    [ComImport]
    [Guid("0000000b-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IComStorage
    {
        void CreateStream(string pwcsName, uint grfMode, uint reserved1, uint reserved2, out IStream ppstm);
        void OpenStream(string pwcsName, IntPtr reserved1, uint grfMode, uint reserved2, out IStream ppstm);
        void CreateStorage(string pwcsName, uint grfMode, uint reserved1, uint reserved2, out IComStorage ppstg);
        void OpenStorage(string pwcsName, IComStorage pstgPriority, uint grfMode, IntPtr snbExclude, uint reserved, out IComStorage ppstg);
        void CopyTo(uint ciidExclude, Guid rgiidExclude, IntPtr snbExclude, IComStorage pstgDest);
        void MoveElementTo(string pwcsName, IComStorage pstgDest, string pwcsNewName, uint grfFlags);
        void Commit(uint grfCommitFlags);
        void Revert();
        void EnumElements(uint reserved1, IntPtr reserved2, uint reserved3, out IEnumSTATSTG ppenum);
        void DestroyElement(string pwcsName);
        void RenameElement(string pwcsOldName, string pwcsNewName);
        void SetElementTimes(string pwcsName, System.Runtime.InteropServices.ComTypes.FILETIME pctime, System.Runtime.InteropServices.ComTypes.FILETIME patime, System.Runtime.InteropServices.ComTypes.FILETIME pmtime);
        void SetClass(Guid clsid);
        void SetStateBits(uint grfStateBits, uint grfMask);
        void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, uint grfStatFlag);
    }

    // #endregion

    /// <summary>
    /// Represents the implementation of Storage in .NET
    /// </summary>
    public class ComStorage : IStorage
    {
        private bool m_IsWritable;

        public IComStorage Storage { get; private set; }

        public ComStorage(IComStorage storage, bool writable) : this(writable)
        {
            Load(storage);
        }

        protected ComStorage(bool writable)
        {
            m_IsWritable = writable;
        }

        protected void Load(IComStorage storage)
        {
            if (storage == null)
            {
                throw new ArgumentNullException(nameof(storage));
            }

            Storage = storage;
        }

        public IStorage TryOpenStorage(string storageName, bool createIfNotExist)
        {
            try
            {
                IComStorage storage;

                Storage.OpenStorage(storageName, null,
                    (uint)Mode, IntPtr.Zero, 0, out storage);

                return new ComStorage(storage, m_IsWritable);
            }
            catch
            {
                if (createIfNotExist)
                {
                    return CreateStorage(storageName);
                }
                else
                {
                    return null;
                }
            }
        }

        public Stream TryOpenStream(string streamName, bool createIfNotExist)
        {
            try
            {
                IStream stream = null;

                Storage.OpenStream(streamName,
                    IntPtr.Zero, (uint)Mode, 0, out stream);

                return new ComStream(stream, m_IsWritable);
            }
            catch
            {
                if (createIfNotExist)
                {
                    return CreateStream(streamName);
                }
                else
                {
                    return null;
                }
            }
        }

        public string[] GetSubStreamNames()
        {
            return EnumElements()
                .Where(e => e.type == (int)STGTY.STREAM)
                .Select(e => e.pwcsName).ToArray();
        }

        public string[] GetSubStorageNames()
        {
            return EnumElements()
                .Where(e => e.type == (int)STGTY.STORAGE)
                .Select(e => e.pwcsName).ToArray();
        }

        private ComStream CreateStream(string streamName)
        {
            IStream stream = null;

            Storage.CreateStream(streamName,
                (uint)STGM.CREATE | (uint)STGM.SHARE_EXCLUSIVE | (uint)STGM.WRITE,
                0, 0, out stream);

            return new ComStream(stream, m_IsWritable);
        }

        private IStorage CreateStorage(string storageName)
        {
            IComStorage storage = null;

            Storage.CreateStorage(storageName,
                (uint)STGM.CREATE | (uint)STGM.SHARE_EXCLUSIVE | (uint)STGM.WRITE,
                0, 0, out storage);

            return new ComStorage(storage, m_IsWritable);
        }

        private IEnumerable<System.Runtime.InteropServices.ComTypes.STATSTG> EnumElements()
        {
            IEnumSTATSTG ssenum = null;

            Storage.EnumElements(0, IntPtr.Zero, 0, out ssenum);

            var ssstruct = new System.Runtime.InteropServices.ComTypes.STATSTG[1];

            uint numReturned;

            do
            {
                ssenum.Next(1, ssstruct, out numReturned);

                if (numReturned != 0)
                {
                    yield return ssstruct[0];
                }
            } while (numReturned > 0);
        }

        public void Close()
        {
            if (Storage != null)
            {
                if (m_IsWritable)
                {
                    Storage.Commit(0);
                }

                Marshal.ReleaseComObject(Storage);
                Storage = null;
                GC.SuppressFinalize(this);
            }
        }

        private STGM Mode
        {
            get
            {
                var mode = STGM.SHARE_EXCLUSIVE;

                if (m_IsWritable)
                {
                    mode |= STGM.READWRITE;
                }

                return mode;
            }
        }

        public void RemoveSubElement(string name)
        {
            Storage.DestroyElement(name);
        }

        public virtual void Dispose()
        {
            Close();
        }
    }
}

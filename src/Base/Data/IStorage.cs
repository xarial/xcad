//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Xarial.XCad.Data
{
    public interface IStorage : IDisposable
    {
        IStorage TryOpenStorage(string storageName, bool createIfNotExist);
        Stream TryOpenStream(string streamName, bool createIfNotExist);
        string[] GetSubStreamNames();
        string[] GetSubStorageNames();
        void RemoveSubElement(string name);
    }
}

//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Xarial.XCad.Data
{
    /// <summary>
    /// Represents the data storage
    /// </summary>
    /// <remarks>This is used as the 3rd party storage of the data in the models</remarks>
    public interface IStorage : IDisposable
    {
        /// <summary>
        /// Attempts to open the storage
        /// </summary>
        /// <param name="storageName">Name of the storage</param>
        /// <param name="createIfNotExist">Create new storage if does not exist</param>
        /// <returns>Storage or null</returns>
        IStorage TryOpenStorage(string storageName, bool createIfNotExist);

        /// <summary>
        /// Attemps to open stream from this storage
        /// </summary>
        /// <param name="streamName">Name of the stream</param>
        /// <param name="createIfNotExist">Create new stream if not exist</param>
        /// <returns>Stream or null</returns>
        Stream TryOpenStream(string streamName, bool createIfNotExist);

        /// <summary>
        /// Returns the names of all sub-streams
        /// </summary>
        /// <returns></returns>
        string[] GetSubStreamNames();

        /// <summary>
        /// Returns the names of all sub-storages
        /// </summary>
        /// <returns></returns>
        string[] GetSubStorageNames();

        /// <summary>
        /// Removes the specified sub-storage or sub-stream
        /// </summary>
        /// <param name="name">Name of the stream or storage</param>
        void RemoveSubElement(string name);
    }
}

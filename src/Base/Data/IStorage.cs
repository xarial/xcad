//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// <returns>Storage or <see cref="Storage.Null"/></returns>
        IStorage OpenStorage(string storageName, bool createIfNotExist);

        /// <summary>
        /// Attemps to open stream from this storage
        /// </summary>
        /// <param name="streamName">Name of the stream</param>
        /// <param name="createIfNotExist">Create new stream if not exist</param>
        /// <returns>Stream or <see cref="Stream.Null"/></returns>
        Stream OpenStream(string streamName, bool createIfNotExist);

        /// <summary>
        /// Returns the names of all sub-streams
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> SubStreamNames { get; }

        /// <summary>
        /// Returns the names of all sub-storages
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> SubStorageNames { get; }

        /// <summary>
        /// Removes the specified sub-storage or sub-stream
        /// </summary>
        /// <param name="name">Name of the stream or storage</param>
        void RemoveSubElement(string name);
    }

    /// <summary>
    /// Constants of <see cref="IStorage"/>
    /// </summary>
    public static class Storage 
    {
        /// <summary>
        /// Storage with no backing store
        /// </summary>
        public static IStorage Null { get; }

        static Storage() 
        {
            Null = new NullStorage();
        }

        private class NullStorage : IStorage
        {
            public IEnumerable<string> SubStorageNames
                => Enumerable.Empty<string>();

            public IEnumerable<string> SubStreamNames
                => Enumerable.Empty<string>();

            public void RemoveSubElement(string name)
            {
            }

            public IStorage OpenStorage(string storageName, bool createIfNotExist)
                => Null;

            public Stream OpenStream(string streamName, bool createIfNotExist)
                => Stream.Null;

            public void Dispose()
            {
            }
        }
    }
}

//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.IO;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Enums;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Features;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents the base interface of all document types
    /// </summary>
    public interface IXDocument : IXTransaction
    {
        /// <summary>
        /// Fired when user data stream is available for reading
        /// </summary>
        event DataStoreAvailableDelegate StreamReadAvailable;

        /// <summary>
        /// Fired when user data storage is available for reading
        /// </summary>
        event DataStoreAvailableDelegate StorageReadAvailable;

        /// <summary>
        /// Fired when user data stream is available for writing
        /// </summary>
        event DataStoreAvailableDelegate StreamWriteAvailable;

        /// <summary>
        /// Fired when user data storage is available for writing
        /// </summary>
        event DataStoreAvailableDelegate StorageWriteAvailable;

        /// <summary>
        /// Fired when document is rebuilt
        /// </summary>
        event DocumentRebuildDelegate Rebuild;

        /// <summary>
        /// Fired when documetn is saving
        /// </summary>
        event DocumentSaveDelegate Saving;

        /// <summary>
        /// Fired when document is closing
        /// </summary>
        event DocumentCloseDelegate Closing;

        /// <summary>
        /// Changes the title of this document
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Path to the document (if saved)
        /// </summary>
        string Path { get; set; }

        /// <summary>
        /// Checks if document has any unsaved changes
        /// </summary>
        bool IsDirty { get; set; }

        /// <summary>
        /// Checks if document is visible
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// Opens document in read-only mode
        /// </summary>
        bool ReadOnly { get; set; }

        /// <summary>
        /// Opens document in view only mode
        /// </summary>
        bool ViewOnly { get; set; }

        /// <summary>
        /// Opens document without displaying any popup messages
        /// </summary>
        bool Silent { get; set; }

        /// <summary>
        /// Opens document in the rapid mode
        /// </summary>
        /// <remarks>This mode significantly improves the performance of opening but certain functionality and API migth not be available</remarks>
        bool Rapid { get; set; }

        /// <summary>
        /// Provides an ability to store temp tags in this session
        /// </summary>
        ITagsManager Tags { get; }

        /// <summary>
        /// Closes this document
        /// </summary>
        void Close();

        /// <summary>
        /// Collection of features of this document
        /// </summary>
        IXFeatureRepository Features { get; }

        /// <summary>
        /// Collection of selections of this document
        /// </summary>
        IXSelectionRepository Selections { get; }

        /// <summary>
        /// Collection of dimensions of this document
        /// </summary>
        IXDimensionRepository Dimensions { get; }

        /// <summary>
        /// Collection of proeprties of this document
        /// </summary>
        IXPropertyRepository Properties { get; }

        /// <summary>
        /// Opens the user data stream from this document
        /// </summary>
        /// <param name="name">Name of the stream</param>
        /// <param name="access">Access type</param>
        /// <returns>Pointer to stream</returns>
        Stream OpenStream(string name, AccessType_e access);
        
        /// <summary>
        /// Opens the user data storage from this document
        /// </summary>
        /// <param name="name">Name of the storage</param>
        /// <param name="access">Access type</param>
        /// <returns>Pointer to the storage</returns>
        IStorage OpenStorage(string name, AccessType_e access);
    }

    public interface IXUnknownDocument : IXDocument 
    {
        IXDocument GetSpecific();
    }
}
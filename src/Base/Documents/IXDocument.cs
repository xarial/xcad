//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.ComponentModel;
using System.IO;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Enums;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Features;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents the base interface of all document types
    /// </summary>
    public interface IXDocument : IXObject, IXTransaction, IPropertiesOwner
    {
        /// <summary>
        /// Current version of the document
        /// </summary>
        IXVersion Version { get; }

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
        /// Document template
        /// </summary>
        string Template { get; set; }

        /// <summary>
        /// Path to the document (if saved)
        /// </summary>
        string Path { get; set; }

        /// <summary>
        /// Checks if document has any unsaved changes
        /// </summary>
        bool IsDirty { get; set; }

        /// <summary>
        /// Gets or sets the state of the document
        /// </summary>
        DocumentState_e State { get; set; }

        /// <summary>
        /// Provides an ability to store temp tags in this session
        /// </summary>
        ITagsManager Tags { get; }

        /// <summary>
        /// Closes this document
        /// </summary>
        void Close();

        /// <summary>
        /// Saves this document
        /// </summary>
        void Save();

        /// <summary>
        /// Identifies if the pointer to the document is still valid
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Saves this document to a new location
        /// </summary>
        /// <param name="filePath"></param>
        void SaveAs(string filePath);

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

        /// <summary>
        /// Returns dependencies of this document
        /// </summary>
        /// <remarks>Dependencies might be uncommited if document is loaded view only or in the rapid mode. Use <see cref="IXTransaction.IsCommitted"/> to check the state and call <see cref="IXTransaction.Commit(System.Threading.CancellationToken)"/> to load document if needed</remarks>
        IXDocument3D[] Dependencies { get; }

        /// <summary>
        /// Deserializes specific object from stream
        /// </summary>
        /// <param name="stream">Input stream with the serialized object</param>
        /// <returns>Deserialized object</returns>
        IXObject DeserializeObject(Stream stream);
    }

    /// <summary>
    /// Represents the unknown document type
    /// </summary>
    public interface IXUnknownDocument : IXDocument 
    {
        /// <summary>
        /// Retrieves the specific document from the unknown document
        /// </summary>
        /// <returns></returns>
        IXDocument GetSpecific();
    }
}
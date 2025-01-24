﻿//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.Features;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents the base interface of all document types
    /// </summary>
    public interface IXDocument : IXObject, IXTransaction, IDimensionable, IDisposable
    {
        /// <summary>
        /// Id of this document
        /// </summary>
        IXIdentifier Id { get; }

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
        event DocumentEventDelegate Rebuilt;

        /// <summary>
        /// Fired when documetn is saving
        /// </summary>
        event DocumentSaveDelegate Saving;

        /// <summary>
        /// Fired when document is saved
        /// </summary>
        event DocumentSavedDelegate Saved;

        /// <summary>
        /// Fired when document is closing
        /// </summary>
        event DocumentCloseDelegate Closing;

        /// <summary>
        /// Fired when document is destroyed
        /// </summary>
        event DocumentEventDelegate Destroyed;

        /// <summary>
        /// Units assigned in this document
        /// </summary>
        IXUnits Units { get; }

        /// <summary>
        /// Document specific options
        /// </summary>
        IXDocumentOptions Options { get; }

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
        /// Returns views collection
        /// </summary>
        IXModelViewRepository ModelViews { get; }

        /// <summary>
        /// Closes this document
        /// </summary>
        void Close();

        /// <summary>
        /// Saves this document
        /// </summary>
        /// <exception cref="Exceptions.SaveNeverSavedDocumentException"/>
        /// <exception cref="Exceptions.SaveDocumentFailedException"/>
        void Save();

        /// <summary>
        /// Pre-creates save-as operation
        /// </summary>
        /// <param name="filePath"></param>
        IXSaveOperation PreCreateSaveAsOperation(string filePath);

        /// <summary>
        /// Collection of properties
        /// </summary>
        IXPropertyRepository Properties { get; }

        /// <summary>
        /// Collection of annotations
        /// </summary>
        IXAnnotationRepository Annotations { get; }

        /// <summary>
        /// Collection of features of this document
        /// </summary>
        IXFeatureRepository Features { get; }

        /// <summary>
        /// Collection of selections of this document
        /// </summary>
        IXSelectionRepository Selections { get; }

        /// <summary>
        /// Opens the user data stream from this document
        /// </summary>
        /// <param name="name">Name of the stream</param>
        /// <param name="write">True to open for writing, False to open for reading</param>
        /// <returns>Pointer to stream or <see cref="Stream.Null"/> if stream does not exist</returns>
        Stream OpenStream(string name, bool write);

        /// <summary>
        /// Opens the user data storage from this document
        /// </summary>
        /// <param name="name">Name of the storage</param>
        /// <param name="write">True to open for writing, False to open for reading</param>
        /// <returns>Pointer to the storage or <see cref="Storage.Null"/></returns>
        IStorage OpenStorage(string name, bool write);

        /// <summary>
        /// Returns top level dependencies of this document
        /// </summary>
        /// <remarks>Dependencies might be uncommited if document is loaded view only or in the rapid mode. Use <see cref="IXTransaction.IsCommitted"/> to check the state and call <see cref="IXTransaction.Commit(System.Threading.CancellationToken)"/> to load document if needed.
        /// In most CADs this method will work with uncommitted documents</remarks>
        IXDocumentDependencies Dependencies { get; }

        /// <summary>
        /// Deserializes specific object from stream
        /// </summary>
        /// <param name="stream">Input stream with the serialized object</param>
        /// <returns>Deserialized object</returns>
        TObj DeserializeObject<TObj>(Stream stream)
            where TObj : IXObject;

        /// <summary>
        /// Regenerates this document
        /// </summary>
        void Rebuild();

        /// <summary>
        /// Starts the group of opertions
        /// </summary>
        /// <returns>Operation group template</returns>
        IOperationGroup PreCreateOperationGroup();

        /// <summary>
        /// Returns the time stamp of the change of the current model
        /// </summary>
        int UpdateStamp { get; }
    }

    /// <summary>
    /// Represents the unknown document type
    /// </summary>
    /// <remarks>This interface provides an access to the document whose specific type cannot be determined in advance
    /// (e.g. imported document types might be both parts and assemblies and it is not known until the document is opened)</remarks>
    public interface IXUnknownDocument : IXDocument
    {
        /// <summary>
        /// Retrieves the specific document from the unknown document
        /// </summary>
        /// <returns></returns>
        IXDocument GetSpecific();
    }
}
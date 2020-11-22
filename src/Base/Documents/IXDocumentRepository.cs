//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using Xarial.XCad.Base;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.Documents.Structures;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents the collection of documents for this application
    /// </summary>
    public interface IXDocumentRepository : IXRepository<IXDocument>
    {
        /// <summary>
        /// Fired when document is activated
        /// </summary>
        event DocumentActivateDelegate DocumentActivated;
        
        /// <summary>
        /// Fired when new document is created
        /// </summary>
        event DocumentCreateDelegate DocumentCreated;

        /// <summary>
        /// Returns the pointer to active document
        /// </summary>
        IXDocument Active { get; set; }

        /// <summary>
        /// Registers document handler
        /// </summary>
        /// <typeparam name="THandler"></typeparam>
        void RegisterHandler<THandler>() where THandler : IDocumentHandler, new();

        /// <summary>
        /// Returns the handler for this document
        /// </summary>
        /// <typeparam name="THandler">Handler type</typeparam>
        /// <param name="doc">Document to get handler from</param>
        /// <returns>Instance of the handler</returns>
        THandler GetHandler<THandler>(IXDocument doc) where THandler : IDocumentHandler, new();

        /// <summary>
        /// Pre-creates a document template
        /// </summary>
        /// <typeparam name="TDocument">Document type to pre-create</typeparam>
        /// <returns>Document template</returns>
        TDocument PreCreate<TDocument>() where TDocument : class, IXDocument;
    }
}
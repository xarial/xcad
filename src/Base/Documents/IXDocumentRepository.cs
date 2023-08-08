//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
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
        event DocumentEventDelegate DocumentActivated;
        
        /// <summary>
        /// Fired when new document is loaded (opened or new document is created)
        /// </summary>
        /// <remarks>This event is fired for all referenced documents (e.g. assembly components or drawing view referenced models)
        /// Document might not be fully loaded at this point
        /// This event is fired before <see cref="DocumentOpened"/> and <see cref="NewDocumentCreated"/>
        /// </remarks>
        event DocumentEventDelegate DocumentLoaded;

        /// <summary>
        /// Fired when top-level document is opened
        /// </summary>
        /// <remarks>Unlike <see cref="DocumentLoaded"/> event, this even will only be fired for the top document (part, assembly or drawing) but not for the references. This event is fired after the <see cref="DocumentLoaded"/></remarks>
        event DocumentEventDelegate DocumentOpened;

        /// <summary>
        /// Fired when new document is created
        /// </summary>
        /// <remarks>This event is fired after the <see cref="DocumentLoaded"/></remarks>
        event DocumentEventDelegate NewDocumentCreated;

        /// <summary>
        /// Returns the pointer to active document
        /// </summary>
        IXDocument Active { get; set; }

        /// <summary>
        /// Registers document handler
        /// </summary>
        /// <param name="handlerFact">Handler factory</param>
        /// <typeparam name="THandler"></typeparam>
        void RegisterHandler<THandler>(Func<THandler> handlerFact)
            where THandler : IDocumentHandler;

        /// <summary>
        /// Unregisters document handler
        /// </summary>
        /// <typeparam name="THandler"></typeparam>
        void UnregisterHandler<THandler>() where THandler : IDocumentHandler;

        /// <summary>
        /// Returns the handler for this document
        /// </summary>
        /// <typeparam name="THandler">Handler type</typeparam>
        /// <param name="doc">Document to get handler from</param>
        /// <returns>Instance of the handler</returns>
        THandler GetHandler<THandler>(IXDocument doc) where THandler : IDocumentHandler;
    }
}
//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.Documents.Structures;

namespace Xarial.XCad.Documents
{
    public interface IXDocumentCollection : IEnumerable<IXDocument>
    {
        event DocumentActivateDelegate DocumentActivated;
        event DocumentCreateDelegate DocumentCreated;

        IXDocument Active { get; set; }

        IXDocument Open(DocumentOpenArgs args);

        int Count { get; }

        void RegisterHandler<THandler>() where THandler : IDocumentHandler, new();
        THandler GetHandler<THandler>(IXDocument doc) where THandler : IDocumentHandler, new();
    }
}
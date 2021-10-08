//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Documents.Services
{
    /// <summary>
    /// Document handler to be used in <see cref="IDocumentsHandler{TDocHandler}"/> documents manager
    /// </summary>
    public interface IDocumentHandler : IDisposable
    {
        /// <summary>
        /// Called when model document is initialized (created)
        /// </summary>
        /// <param name="app">Pointer to application</param>
        /// <param name="model">Pointer to this model document</param>
        void Init(IXApplication app, IXDocument model);
    }
}

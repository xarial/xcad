//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.SolidWorks.Documents.Exceptions;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Documents.Services
{
    /// <remarks>
    /// DocumentLoadNotify2 even is fired async so it is not ensured that it is raised before or after OpenDoc6 or NewDocument APIs. This services is responsible for handling the race conditions
    /// </remarks>
    internal class SwDocumentDispatcher : DocumentDispatcherBase<SwDocument, IModelDoc2>
    {
        private readonly SwApplication m_App;

        internal SwDocumentDispatcher(SwApplication app, IXLogger logger) : base(logger)
        {
            m_App = app;
        }

        protected override void BindDocument(SwDocument doc, IModelDoc2 underlineDoc)
        {
            if (!doc.IsCommitted)
            {
                doc.SetModel(underlineDoc);
            }

            if (doc.IsCommitted)
            {
                if (!(doc is SwUnknownDocument))
                {
                    doc.AttachEvents();
                }
            }
        }

        //NOTE: it might not be enough to compare the pointers. When LoadNotify2 event is called from different threads pointers might not be 
        protected override bool CompareNativeDocuments(IModelDoc2 firstDoc, IModelDoc2 secondDoc)
            => m_App.Sw.IsSame(firstDoc, secondDoc) == (int)swObjectEquality.swObjectSame;

        protected override SwDocument CreateDocument(IModelDoc2 underlineDoc)
        {
            switch (underlineDoc)
            {
                case IPartDoc part:
                    return new SwPart(part, m_App, m_Logger, true);

                case IAssemblyDoc assm:
                    return new SwAssembly(assm, m_App, m_Logger, true);

                case IDrawingDoc drw:
                    return new SwDrawing(drw, m_App, m_Logger, true);

                default:
                    throw new NotSupportedException($"Invalid cast of '{underlineDoc.GetPathName()}' [{underlineDoc.GetTitle()}] of type '{((object)underlineDoc).GetType().FullName}'. Specific document type: {(swDocumentTypes_e)underlineDoc.GetType()}");
            }
        }

        protected override string GetTitle(IModelDoc2 underlineDoc) => underlineDoc.GetTitle();
    }
}

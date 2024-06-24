//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Documents.EventHandlers
{
    internal class DocumentSavedEventHandler : SwModelEventsHandler<DocumentSavedDelegate>
    {
        internal DocumentSavedEventHandler(SwDocument doc, ISwApplication app) : base(doc, app)
        {
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.FileSavePostNotify += OnFileSavePostNotify;
            assm.FileSavePostCancelNotify += OnFileSavePostCancelNotify;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.FileSavePostNotify += OnFileSavePostNotify;
            drw.FileSavePostCancelNotify += OnFileSavePostCancelNotify;
        }

        protected override void SubscribePartEvents(PartDoc part)
        {
            part.FileSavePostNotify += OnFileSavePostNotify;
            part.FileSavePostCancelNotify += OnFileSavePostCancelNotify;
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.FileSavePostNotify -= OnFileSavePostNotify;
            assm.FileSavePostCancelNotify -= OnFileSavePostCancelNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.FileSavePostNotify -= OnFileSavePostNotify;
            drw.FileSavePostCancelNotify -= OnFileSavePostCancelNotify;
        }

        protected override void UnsubscribePartEvents(PartDoc part)
        {
            part.FileSavePostNotify -= OnFileSavePostNotify;
            part.FileSavePostCancelNotify -= OnFileSavePostCancelNotify;
        }

        private int OnFileSavePostNotify(int saveType, string fileName)
        {
            DocumentSaveType_e docSaveType;

            switch ((swFileSaveTypes_e)saveType) 
            {
                case swFileSaveTypes_e.swFileSave:
                    docSaveType = DocumentSaveType_e.SaveCurrent;
                    break;

                case swFileSaveTypes_e.swFileSaveAs:
                case swFileSaveTypes_e.swFileSaveAsCopy:
                case swFileSaveTypes_e.swFileSaveAsCopyAndOpen:
                    docSaveType = DocumentSaveType_e.SaveAs;
                    break;

                default:
                    docSaveType = (DocumentSaveType_e)(-1);
                    break;
            }

            Delegate?.Invoke(m_Doc, docSaveType, false);

            return HResult.S_OK;
        }

        private int OnFileSavePostCancelNotify()
        {
            Delegate?.Invoke(m_Doc, (DocumentSaveType_e)(-1), true);

            return HResult.S_OK;
        }
    }
}

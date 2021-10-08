//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
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
    internal class DocumentSavingEventHandler : SwModelEventsHandler<DocumentSaveDelegate>
    {
        internal DocumentSavingEventHandler(SwDocument doc, ISwApplication app) : base(doc, app)
        {
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.FileSaveAsNotify2 += OnFileSaveAsNotify2;
            assm.FileSaveNotify += OnFileSaveNotify;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.FileSaveAsNotify2 += OnFileSaveAsNotify2;
            drw.FileSaveNotify += OnFileSaveNotify;
        }

        protected override void SubscribePartEvents(PartDoc part)
        {
            part.FileSaveAsNotify2 += OnFileSaveAsNotify2;
            part.FileSaveNotify += OnFileSaveNotify;
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.FileSaveAsNotify2 -= OnFileSaveAsNotify2;
            assm.FileSaveNotify -= OnFileSaveNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc drw)
        {
            drw.FileSaveAsNotify2 -= OnFileSaveAsNotify2;
            drw.FileSaveNotify -= OnFileSaveNotify;
        }

        protected override void UnsubscribePartEvents(PartDoc part)
        {
            part.FileSaveAsNotify2 -= OnFileSaveAsNotify2;
            part.FileSaveNotify -= OnFileSaveNotify;
        }

        private int OnFileSaveNotify(string fileName)
            => HandleEvent(fileName, DocumentSaveType_e.SaveCurrent);

        private int OnFileSaveAsNotify2(string fileName) 
            => HandleEvent(fileName, DocumentSaveType_e.SaveAs);

        private int HandleEvent(string fileName, DocumentSaveType_e type)
        {
            var args = new DocumentSaveArgs()
            {
                FileName = fileName,
                Cancel = false
            };

            Delegate?.Invoke(m_Doc, type, args);

            if (args.FileName != fileName) 
            {
                if (type == DocumentSaveType_e.SaveAs)
                {
                    m_Doc.Model.SetSaveAsFileName(args.FileName);
                    return S_FALSE;
                }
                else if (type == DocumentSaveType_e.SaveCurrent) 
                {
                    throw new NotSupportedException("File name can be changed for SaveAs file only");
                }
            }

            return args.Cancel ? S_FALSE : S_OK;
        }
    }
}

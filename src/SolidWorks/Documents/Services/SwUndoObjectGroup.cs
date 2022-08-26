//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Documents.Services;

namespace Xarial.XCad.SolidWorks.Documents.Services
{
    internal class SwUndoObjectGroup : IOperationGroup
    {
        public string Name { get; set; }
        public bool IsTemp { get; set; }

        public bool IsCommitted { get; private set; }

        private readonly ISwDocument m_Doc;

        internal SwUndoObjectGroup(ISwDocument doc) 
        {
            m_Doc = doc;
        }

        public void Commit(CancellationToken cancellationToken)
        {
            IsCommitted = true;
            m_Doc.Model.Extension.StartRecordingUndoObject();
        }

        public void Dispose()
        {
            if (IsCommitted) 
            {
                if (!m_Doc.Model.Extension.FinishRecordingUndoObject2(Name, false)) 
                {
                    throw new Exception("Failed to finish recording undo object");
                }

                if (IsTemp) 
                {
                    m_Doc.Model.EditUndo2(1);
                }
            }
        }
    }
}

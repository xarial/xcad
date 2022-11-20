using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Services;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    internal class SwDmSaveOperation : IXSaveOperation
    {
        public string FilePath { get; }

        public bool IsCommitted => m_Creator.IsCreated;

        protected readonly ElementCreator<bool?> m_Creator;

        protected readonly SwDmDocument m_Doc;

        internal SwDmSaveOperation(SwDmDocument doc, string filePath)
        {
            m_Doc = doc;
            FilePath = filePath;

            m_Creator = new ElementCreator<bool?>(SaveAs, null, false);
        }

        private bool? SaveAs(CancellationToken cancellationToken)
        {
            m_Doc.PerformSave(DocumentSaveType_e.SaveAs, FilePath, f => true, (d, f) => d.SaveAs(f));
            return true;
        }

        public void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);
    }
}

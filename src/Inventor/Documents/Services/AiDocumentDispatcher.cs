using Inventor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.Inventor.Documents.Services
{
    internal class AiDocumentDispatcher : DocumentDispatcherBase<AiDocument, Document>
    {
        private readonly AiApplication m_App;

        internal AiDocumentDispatcher(AiApplication app, IXLogger logger) : base(logger)
        {
            m_App = app;
        }

        protected override void BindDocument(AiDocument doc, Document underlineDoc)
        {
            if (!doc.IsCommitted)
            {
                doc.SetModel(underlineDoc);
            }
        }

        protected override bool CompareNativeDocuments(Document firstDoc, Document secondDoc)
            => string.Equals(firstDoc.InternalName, secondDoc.InternalName);

        protected override AiDocument CreateDocument(Document specDoc)
            => new AiDocument(specDoc, m_App);

        protected override string GetTitle(Document underlineDoc)
            => underlineDoc.DisplayName;
    }
}

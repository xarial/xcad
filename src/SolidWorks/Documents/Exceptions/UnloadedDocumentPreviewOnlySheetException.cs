using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.Documents.Exceptions
{
    public class UnloadedDocumentPreviewOnlySheetException : NotSupportedException
    {
        public UnloadedDocumentPreviewOnlySheetException()
            : base("Active sheet of uncommitted document can only be used to extract preview")
        {
        }
    }
}

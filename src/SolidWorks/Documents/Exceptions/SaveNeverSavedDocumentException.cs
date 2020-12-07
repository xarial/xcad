using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.Documents.Exceptions
{
    public class SaveNeverSavedDocumentException : Exception
    {
        public SaveNeverSavedDocumentException() : base("Model never saved use SaveAs instead") 
        {
        }
    }
}

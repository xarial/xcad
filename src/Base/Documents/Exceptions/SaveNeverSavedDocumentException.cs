//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Documents.Exceptions
{
    public class SaveNeverSavedDocumentException : Exception
    {
        public SaveNeverSavedDocumentException() : base("Model never saved use SaveAs instead") 
        {
        }
    }
}

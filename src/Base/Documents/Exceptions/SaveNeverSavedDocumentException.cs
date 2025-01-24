//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Documents.Exceptions
{
    /// <summary>
    /// Exception when document is attempted to be saved as current while it was never saved before
    /// </summary>
    public class SaveNeverSavedDocumentException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SaveNeverSavedDocumentException() : base("Model never saved use SaveAs instead") 
        {
        }
    }
}

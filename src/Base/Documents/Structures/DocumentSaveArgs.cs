//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Documents.Structures
{
    /// <summary>
    /// Argument passed with <see cref="IXDocument.Saving"/> event
    /// </summary>
    public class DocumentSaveArgs
    {
        /// <summary>
        /// Overrides the save as file name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Specifies if saving operation needs to be cancelled
        /// </summary>
        public bool Cancel { get; set; }
    }
}

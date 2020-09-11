//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Documents.Structures
{
    public class DocumentSaveArgs
    {
        public string FileName { get; set; }
        public bool Cancel { get; set; }
    }
}

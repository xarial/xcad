//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.Documents
{
    public class DocumentAlreadyOpenedException : Exception
    {
        public DocumentAlreadyOpenedException(string path) : base($"{path} document already opened") 
        {
        }
    }
}

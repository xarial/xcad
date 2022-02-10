//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Toolkit.PageBuilder.Exceptions
{
    public class DuplicateMetadataTagException : Exception
    {
        public DuplicateMetadataTagException(object tag) : base($"'{tag?.ToString()}' tag already assigned to metadata") 
        {
        }
    }
}

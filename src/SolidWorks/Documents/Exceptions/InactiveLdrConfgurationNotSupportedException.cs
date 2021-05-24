//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SolidWorks.Documents.Exceptions
{
    public class InactiveLdrConfgurationNotSupportedException : NotSupportedException, IUserException
    {
        public InactiveLdrConfgurationNotSupportedException() 
            : base("Inactive configuration of assembly opened in Large Design Review model is not supported and cannot be loaded")
        {
        }
    }
}

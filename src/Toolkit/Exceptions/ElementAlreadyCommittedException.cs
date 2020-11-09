//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Toolkit.Exceptions
{
    public class ElementAlreadyCommittedException : Exception
    {
        public ElementAlreadyCommittedException() : base("This element already committed") 
        {
        }
    }
}

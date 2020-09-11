//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.UI.Exceptions
{
    public class ParentGroupCircularDependencyException : Exception
    {
        public ParentGroupCircularDependencyException(string grpId) 
            : base($"Group cannot be a parent of itself '{grpId}'") 
        {
        }
    }
}

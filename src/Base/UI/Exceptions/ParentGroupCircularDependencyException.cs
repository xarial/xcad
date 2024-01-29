//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.UI.Exceptions
{
    /// <summary>
    /// Exception indicates that the parent group is set as a parent of itself
    /// </summary>
    public class ParentGroupCircularDependencyException : Exception
    {
        public ParentGroupCircularDependencyException(string grpId) 
            : base($"Group cannot be a parent of itself '{grpId}'") 
        {
        }
    }
}

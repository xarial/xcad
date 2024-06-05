//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.UI.Exceptions
{
    /// <summary>
    /// Exception indicates that the specified parent group does not exist
    /// </summary>
    public class ParentGroupNotFoundException : Exception
    {
        public ParentGroupNotFoundException(string parentGrpId, string thisGrpId)
            : base($"Failed to find the parent group '{parentGrpId}' for '{thisGrpId}'") 
        {
        }
    }
}

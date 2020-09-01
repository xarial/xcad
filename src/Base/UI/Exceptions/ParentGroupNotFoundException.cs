using System;

namespace Xarial.XCad.UI.Exceptions
{
    public class ParentGroupNotFoundException : Exception
    {
        public ParentGroupNotFoundException(string parentGrpId, string thisGrpId)
            : base($"Failed to find the parent group '{parentGrpId}' for '{thisGrpId}'") 
        {
        }
    }
}

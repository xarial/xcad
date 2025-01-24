//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.UI.Commands.Attributes
{
    /// <summary>
    /// Associates the parent group for this group
    /// </summary>
    public class CommandGroupParentAttribute : Attribute
    {
        internal int ParentGroupUserId { get; }
        internal Type ParentGroupType { get; }

        /// <param name="parentGroupUserId">User id of the parent group</param>
        public CommandGroupParentAttribute(int parentGroupUserId) 
        {
            ParentGroupUserId = parentGroupUserId;
        }

        /// <param name="parentGroupType">Type of the parent group enumeration</param>
        public CommandGroupParentAttribute(Type parentGroupType)
        {
            if (parentGroupType != null && !parentGroupType.IsEnum)
            {
                throw new InvalidCastException(
                    $"Type '{parentGroupType.FullName}' specified as subgroup must be an enumeration");
            }

            ParentGroupType = parentGroupType;
        }
    }
}

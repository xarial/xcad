//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.UI.Commands.Exceptions
{
    public class DuplicateCommandUserIdsException : Exception
    {
        public DuplicateCommandUserIdsException(string groupTitle, int groupUserId, int[] dupUserIds)
            : base($"The following command user ids are duplicate in the group {groupTitle} [{groupUserId}]: {string.Join(", ", dupUserIds)}") 
        {
        }
    }
}

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

//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Linq;

namespace Xarial.XCad.Utils.PageBuilder.Exceptions
{
    public class InvalidParentControlException : Exception
    {
        internal InvalidParentControlException(Type parentType, params Type[] supportedParents)
            : base($"{parentType.FullName} is not supported as a parent control. Only {string.Join(", ", supportedParents.Select(t => t.FullName).ToArray())} are supported")
        {
        }
    }
}
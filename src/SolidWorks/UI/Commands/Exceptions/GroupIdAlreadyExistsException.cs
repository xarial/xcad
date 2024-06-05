//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Structures;

namespace Xarial.XCad.SolidWorks.UI.Commands.Exceptions
{
    /// <summary>
    /// Exception indicates that specified group user id is already used
    /// </summary>
    /// <remarks>This might happen when <see cref="CommandGroupInfoAttribute"/> explicitly specifies duplicate user ids.
    /// This can also happen that not all commands have this attribute assigned explicitly.
    /// In this case framework is attempting to generate next user id which might be already taken by explicit declaration</remarks>
    public class GroupIdAlreadyExistsException : Exception
    {
        internal GroupIdAlreadyExistsException(CommandGroupSpec cmdBar)
            : base($"Group {cmdBar.Title} id ({cmdBar.Id}) already exists. Make sure that all group enumerators decorated with {typeof(CommandGroupInfoAttribute)} have unique values for id")
        {
        }
    }
}
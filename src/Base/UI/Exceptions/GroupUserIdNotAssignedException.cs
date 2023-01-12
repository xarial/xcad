//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.Commands.Attributes;

namespace Xarial.XCad.UI.Exceptions
{
    /// <summary>
    /// Indicates that no user id assigned in <see cref="XCommandManagerExtension.CreateSpecFromEnum"/>
    /// </summary>
    public class GroupUserIdNotAssignedException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public GroupUserIdNotAssignedException() : base($"User id must be specified or assigned via {typeof(CommandGroupInfoAttribute).FullName} attribute") 
        {
        }
    }
}

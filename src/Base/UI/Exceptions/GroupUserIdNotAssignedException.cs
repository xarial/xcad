using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.Commands.Attributes;

namespace Xarial.XCad.UI.Exceptions
{
    /// <summary>
    /// Indicates that no user id assigned in <see cref="IXCommandManagerExtension.CreateSpecFromEnum"/>
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

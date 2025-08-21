//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.Commands.Enums;

namespace Xarial.XCad.UI.Commands.Attributes
{
    /// <summary>
    /// Provides the additional information about the command group
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    public class CommandGroupInfoAttribute : Attribute
    {
        internal int UserId { get; }
        internal string TabName { get; }
        internal int Position { get; set; }
        internal WorkspaceTypes_e SupportedWorkspace { get; set; }

        /// <inheritdoc cref="CommandGroupInfoAttribute"/>
        public CommandGroupInfoAttribute(int userId) : this(userId, "")
        {
        }

        /// <inheritdoc cref="CommandGroupInfoAttribute"/>
        public CommandGroupInfoAttribute(string tabName) : this(-1, tabName)
        {
        }

        /// <inheritdoc cref="CommandGroupInfoAttribute"/>
        public CommandGroupInfoAttribute(int userId, string tabName) 
            : this(userId, tabName, -1, WorkspaceTypes_e.All)
        {
        }

        /// <summary>
        /// Constructor for specifying the additional information for group
        /// </summary>
        /// <param name="userId">User id for the command group. Must be unique per add-in</param>
        /// <param name="tabName">Name of the tab this group should be added to</param>
        /// <param name="position">Menu position</param>
        /// <param name="suppWorks">Supported workspaces</param>
        public CommandGroupInfoAttribute(int userId, string tabName, int position, WorkspaceTypes_e suppWorks)
        {
            UserId = userId;
            TabName = tabName;
            Position = position;
            SupportedWorkspace = suppWorks;
        }
    }
}
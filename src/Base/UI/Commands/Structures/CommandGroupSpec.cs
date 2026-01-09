//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.Commands.Enums;
using Xarial.XCad.UI.Structures;

namespace Xarial.XCad.UI.Commands.Structures
{
    /// <summary>
    /// Represents the group of commands
    /// </summary>
    public class CommandGroupSpec : ButtonGroupSpec
    {
        /// <summary>
        /// Parent group or null for root group
        /// </summary>
        public CommandGroupSpec Parent { get; set; }

        /// <summary>
        /// Menu position of the command group
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Workspaces where this command group is visible
        /// </summary>
        public WorkspaceTypes_e SupportedWorkspace { get; set; }

        /// <summary>
        /// Id of this group
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Commands associated with this group
        /// </summary>
        public virtual CommandSpec[] Commands { get; set; }

        /// <summary>
        /// Name of the ribbon tab where this group should be added
        /// </summary>
        /// <remarks>If this is not set the name of the group is used or the name of the root parent if this group is a sub group</remarks>
        public string RibbonTabName { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="id">Group id</param>
        public CommandGroupSpec(int id) 
        {
            Id = id;
            Position = -1;
            SupportedWorkspace = WorkspaceTypes_e.All;
        }
    }

    /// <summary>
    /// Represents command groups of the context menu
    /// </summary>
    public class ContextMenuCommandGroupSpec : CommandGroupSpec 
    {
        /// <summary>
        /// Owner of the context menu
        /// </summary>
        public Type Owner { get; set; }

        /// <inheritdoc/>
        public ContextMenuCommandGroupSpec(int id) : base(id)
        {
        }
    }
}
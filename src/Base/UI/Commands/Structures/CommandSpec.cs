//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Drawing;
using Xarial.XCad.UI.Commands.Enums;
using Xarial.XCad.UI.Structures;

namespace Xarial.XCad.UI.Commands.Structures
{
    /// <summary>
    /// Represents the button which is created within the menu or toolbar
    /// </summary>
    public class CommandSpec : ButtonSpec
    {
        /// <summary>
        /// Workspaces where this command is enabled
        /// </summary>
        public WorkspaceTypes_e SupportedWorkspace { get; set; }

        /// <summary>
        /// Indicates if this command should be added to the menu
        /// </summary>
        public bool HasMenu { get; set; }

        /// <summary>
        /// indicates if this command should be added into toolbar
        /// </summary>
        public bool HasToolbar { get; set; }

        /// <summary>
        /// Indicates if this command should be adde to ribbon tab box
        /// </summary>
        public bool HasRibbon { get; set; }

        /// <summary>
        /// Style of the ribbon tab box
        /// </summary>
        public RibbonTabTextDisplay_e RibbonTextStyle { get; set; }

        /// <summary>
        /// Indicates if this command should be separated by spacer
        /// </summary>
        public bool HasSpacer { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="userId">Command user id</param>
        public CommandSpec(int userId) : base(userId) 
        {
        }
    }
}
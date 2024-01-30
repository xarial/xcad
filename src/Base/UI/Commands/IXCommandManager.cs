//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.UI.Commands.Structures;

namespace Xarial.XCad.UI.Commands
{
    /// <summary>
    /// Represents the command manager (toolabr and menus)
    /// </summary>
    public interface IXCommandManager
    {
        /// <summary>
        /// Command groups belonging to this command manager
        /// </summary>
        IXCommandGroup[] CommandGroups { get; }

        /// <summary>
        /// Adds new command group (menu, toolbar or ribbon)
        /// </summary>
        /// <param name="cmdBar">Specification of command group</param>
        /// <returns>Command group</returns>
        IXCommandGroup AddCommandGroup(CommandGroupSpec cmdBar);

        /// <summary>
        /// Adds new context menu
        /// </summary>
        /// <param name="cmdBar">Specification of the context menu</param>
        /// <returns>Command group</returns>
        /// <remarks>Use <see cref="Attributes.ContextMenuCommandGroupInfoAttribute"/> attribute to assign additional parameters for the context menu</remarks>
        IXCommandGroup AddContextMenu(ContextMenuCommandGroupSpec cmdBar);
    }
}
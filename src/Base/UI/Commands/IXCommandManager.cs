//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
        /// <param name="owner">Type where context menu is attached to</param>
        /// <returns>Command group</returns>
        IXCommandGroup AddContextMenu(CommandGroupSpec cmdBar, SelectType_e? owner);
    }
}
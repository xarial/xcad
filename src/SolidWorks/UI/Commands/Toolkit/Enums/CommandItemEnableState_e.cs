//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.SolidWorks.UI.Commands.Toolkit.Enums
{
    /// <summary>
    /// Enumerates the possible states of the command (toolbar button or menu item) in SOLIDWORKS
    /// </summary>
    internal enum CommandItemEnableState_e
    {
        /// <summary>
        /// Deselects and disables the item
        /// </summary>
        DeselectDisable = 0,

        /// <summary>
        /// Deselects and enables the item; this is the default state if no update function is specified
        /// </summary>
        DeselectEnable = 1,

        /// <summary>
        /// Selects and disables the item
        /// </summary>
        SelectDisable = 2,

        /// <summary>
        /// Selects and enables the item
        /// </summary>
        SelectEnable = 3
    }
}
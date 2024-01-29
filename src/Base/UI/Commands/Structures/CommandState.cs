//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.UI.Commands.Structures
{
    /// <summary>
    /// State of the command within the <see cref="IXCommandGroup"/>
    /// </summary>
    public class CommandState
    {
        /// <summary>
        /// Is command enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Is command checked
        /// </summary>
        public bool Checked { get; set; }
    }
}
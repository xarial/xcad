//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.UI.Commands.Delegates;
using Xarial.XCad.UI.Commands.Structures;

namespace Xarial.XCad.UI.Commands
{
    /// <summary>
    /// Represents the group of commands
    /// </summary>
    public interface IXCommandGroup
    {
        /// <summary>
        /// Event raised when the specific command button is clicked
        /// </summary>
        event CommandClickDelegate CommandClick;


        /// <summary>
        /// Event raised when it is required to resolve the state of the button as condition has changed
        /// </summary>
        event CommandStateDelegate CommandStateResolve;

        /// <summary>
        /// Specification of the group
        /// </summary>
        CommandGroupSpec Spec { get; }
    }
}
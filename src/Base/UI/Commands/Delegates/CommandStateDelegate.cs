//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.Commands.Structures;

namespace Xarial.XCad.UI.Commands.Delegates
{
    /// <summary>
    /// Delegate for <see cref="IXCommandGroup.CommandStateResolve"/> event
    /// </summary>
    /// <param name="spec">Command spec</param>
    /// <param name="state">Command state</param>
    public delegate void CommandStateDelegate(CommandSpec spec, CommandState state);

    /// <summary>
    /// Delegate for enum specific <see cref="IXCommandGroup.CommandStateResolve"/> event
    /// </summary>
    /// <typeparam name="TCmdEnum">Enum type</typeparam>
    /// <param name="spec">Command spec enum</param>
    /// <param name="state">Command state</param>
    public delegate void CommandEnumStateDelegate<TCmdEnum>(TCmdEnum spec, CommandState state)
        where TCmdEnum : Enum;
}
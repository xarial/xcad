//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.Commands.Structures;

namespace Xarial.XCad.UI.Commands.Delegates
{
    public delegate void CommandStateDelegate(CommandSpec spec, CommandState state);

    public delegate void CommandEnumStateDelegate<TCmdEnum>(TCmdEnum spec, CommandState state)
        where TCmdEnum : Enum;
}
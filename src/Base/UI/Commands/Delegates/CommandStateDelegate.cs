//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.Commands.Structures;

namespace Xarial.XCad.UI.Commands.Delegates
{
    public delegate void CommandStateDelegate(CommandSpec spec, ref CommandState state);

    public delegate void CommandEnumStateDelegate<TCmdEnum>(TCmdEnum spec, ref CommandState state)
        where TCmdEnum : Enum;
}
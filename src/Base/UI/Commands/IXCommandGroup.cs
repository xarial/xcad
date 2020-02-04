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
    public interface IXCommandGroup
    {
        event CommandClickDelegate CommandClick;

        event CommandStateDelegate CommandStateResolve;

        CommandGroupSpec Spec { get; }
    }
}
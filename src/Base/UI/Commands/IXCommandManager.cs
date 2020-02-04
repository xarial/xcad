//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using Xarial.XCad.UI.Commands.Structures;

namespace Xarial.XCad.UI.Commands
{
    public interface IXCommandManager
    {
        IEnumerable<IXCommandGroup> CommandGroups { get; }

        IXCommandGroup AddCommandGroup(CommandGroupSpec cmdBar);
    }
}
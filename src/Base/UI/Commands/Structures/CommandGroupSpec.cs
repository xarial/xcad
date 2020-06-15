//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Drawing;
using Xarial.XCad.UI.Structures;

namespace Xarial.XCad.UI.Commands.Structures
{
    public class CommandGroupSpec : ButtonGroupSpec
    {
        public CommandGroupSpec Parent { get; set; }
        public int Id { get; set; }
        public CommandSpec[] Commands { get; set; }
    }
}
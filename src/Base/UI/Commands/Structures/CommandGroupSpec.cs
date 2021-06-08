//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.UI.Structures;

namespace Xarial.XCad.UI.Commands.Structures
{
    public class CommandGroupSpec : ButtonGroupSpec
    {
        public CommandGroupSpec Parent { get; set; }
        public int Id { get; }
        public CommandSpec[] Commands { get; set; }

        public CommandGroupSpec(int id) 
        {
            Id = id;
        }
    }
}
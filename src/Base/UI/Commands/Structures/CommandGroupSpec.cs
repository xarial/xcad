//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Drawing;

namespace Xarial.XCad.UI.Commands.Structures
{
    public class CommandGroupSpec
    {
        public CommandGroupSpec Parent { get; set; }
        public string Title { get; set; }
        public string Tooltip { get; set; }
        public Image Icon { get; set; }

        public int Id { get; set; }
        public CommandSpec[] Commands { get; set; }
    }
}
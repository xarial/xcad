//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Drawing;
using Xarial.XCad.UI.Commands.Enums;
using Xarial.XCad.UI.Structures;

namespace Xarial.XCad.UI.Commands.Structures
{
    public class CommandSpec : ButtonSpec
    {
        public WorkspaceTypes_e SupportedWorkspace { get; set; }
        public bool HasMenu { get; set; }
        public bool HasToolbar { get; set; }
        public bool HasTabBox { get; set; }
        public RibbonTabTextDisplay_e TabBoxStyle { get; set; }
        public bool HasSpacer { get; set; }

        public CommandSpec(int userId) : base(userId) 
        {
        }
    }
}
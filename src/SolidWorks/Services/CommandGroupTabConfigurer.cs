//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI.Commands.Structures;

namespace Xarial.XCad.SolidWorks.Services
{
    public class CommandGroupTabConfiguration 
    {
        public string TabName { get; set; }
        public bool Include { get; set; }
    }

    public interface ICommandGroupTabConfigurer
    {
        void ConfigureTab(CommandGroupSpec cmdGrpSpec, CommandGroupTabConfiguration config);
    }

    internal class DefaultCommandGroupTabConfigurer : ICommandGroupTabConfigurer
    {
        public void ConfigureTab(CommandGroupSpec cmdGrpSpec, CommandGroupTabConfiguration config)
        {
        }
    }
}

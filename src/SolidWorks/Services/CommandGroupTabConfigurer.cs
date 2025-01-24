//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI.Commands.Structures;

namespace Xarial.XCad.SolidWorks.Services
{
    /// <summary>
    /// Configuration information of the tab
    /// </summary>
    public class CommandGroupTabConfiguration 
    {
        /// <summary>
        /// Name of the tab
        /// </summary>
        public string TabName { get; set; }

        /// <summary>
        /// True if the tab should be created, False if not
        /// </summary>
        public bool Include { get; set; }
    }

    /// <summary>
    /// Service to configure tabs
    /// </summary>
    public interface ICommandGroupTabConfigurer
    {
        /// <summary>
        /// Called when command tab is created
        /// </summary>
        /// <param name="cmdGrpSpec">Specification of the group</param>
        /// <param name="config">Configuration of the tab</param>
        void ConfigureTab(CommandGroupSpec cmdGrpSpec, CommandGroupTabConfiguration config);
    }

    /// <summary>
    /// Default tab configuration
    /// </summary>
    /// <remarks>This configurer uses the default options and acts as a placeholder.
    /// User can register custom <see cref="ICommandGroupTabConfigurer"/> service to configure the behavior of tabs</remarks>
    internal class DefaultCommandGroupTabConfigurer : ICommandGroupTabConfigurer
    {
        public void ConfigureTab(CommandGroupSpec cmdGrpSpec, CommandGroupTabConfiguration config)
        {
        }
    }
}

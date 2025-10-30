//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Services;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage.Services;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal static class XServiceCollectionExtension
    {
        internal static void RegisterCommon(this IXServiceCollection svc, IXApplication app, bool replace)
        {
            svc.Add<IIconsCreator, BaseIconsCreator>(ServiceLifetimeScope_e.Singleton, replace);
            svc.Add<IHelpLinkHandler>(() => new HelpLinkHandler(app), ServiceLifetimeScope_e.Singleton, replace);
            svc.Add<ITooltipLinkLinkHandler>(() => new TooltipLinkLinkHandler(app), ServiceLifetimeScope_e.Singleton, replace);
        }
    }
}

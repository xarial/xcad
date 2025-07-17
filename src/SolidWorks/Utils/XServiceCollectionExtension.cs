using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage.Services;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal static class XServiceCollectionExtension
    {
        internal static void RegisterCommon(this IXServiceCollection svc, IXApplication app, bool replace, string workDir)
        {
            svc.Add<IIconsCreator, BaseIconsCreator>(ServiceLifetimeScope_e.Singleton, replace);
            svc.Add<IHelpLinkHandler>(() => new HelpLinkHandler(app, workDir), ServiceLifetimeScope_e.Singleton, replace);
        }
    }
}

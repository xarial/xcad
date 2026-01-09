using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Constructors;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Constructors
{
    internal class WpfPropertyManagerPageConstructor : PageConstructor<WpfPropertyManagerPagePage>
    {
        private readonly IIconsCreator m_IconConv;
        private readonly IHelpLinkHandler m_HelpLinkHandler;

        internal WpfPropertyManagerPageConstructor(IIconsCreator iconConv, IHelpLinkHandler helpLinkHandler)
        {
            m_IconConv = iconConv;
            m_HelpLinkHandler = helpLinkHandler;
        }

        protected override WpfPropertyManagerPagePage Create(IAttributeSet atts)
            => new WpfPropertyManagerPagePage(atts, m_IconConv, m_HelpLinkHandler);
    }
}

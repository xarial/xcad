//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Toolkit.PageBuilder.Services;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Constructors;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.Utils.PageBuilder;
using Xarial.XCad.Utils.PageBuilder.Binders;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit
{
    internal class WpfPropertyManagerPageBuilder
        : PageBuilderBase<WpfPropertyManagerPagePage, WpfPropertyManagerPageGroup, IWpfPropertyManagerPageControl>
    {
        public WpfPropertyManagerPageBuilder(IXApplication app, IDynamicControlFactoryProvider dynCtrlFactProv,
            IIconsCreator iconConv, IHelpLinkHandler helpLinkHandler, IXLogger logger)
            : this(app, new TypeDataBinder(dynCtrlFactProv, logger), iconConv, helpLinkHandler)
        {
        }

        private WpfPropertyManagerPageBuilder(IXApplication app, TypeDataBinder dataBinder, IIconsCreator iconConv, IHelpLinkHandler helpLinkHandler)
            : base(app, dataBinder,
                  new WpfPropertyManagerPageConstructor(iconConv, helpLinkHandler),
                  new WpfPropertyManagerPageGroupConstructor(),
                  new WpfPropertyManagerPageTextBoxConstructor(app, iconConv),
                  new WpfPropertyManagerPageNumberBoxConstructor(app, iconConv),
                  new WpfPropertyManagerPageCheckBoxConstructor(app, iconConv),
                  new WpfPropertyManagerPageEnumComboBoxConstructor(app, iconConv),
                  new WpfPropertyManagerPageCustomItemsComboBoxConstructor(app, iconConv),
                  new WpfPropertyManagerPageListBoxConstructor(app, iconConv),
                  new WpfPropertyManagerPageSelectionBoxConstructor(app, iconConv),
                  new WpfPropertyManagerPageOptionBoxConstructor(app, iconConv),
                  new WpfPropertyManagerPageCheckBoxListConstructor(app, iconConv),
                  new WpfPropertyManagerPageButtonConstructor(app, iconConv),
                  new WpfPropertyManagerPageBitmapConstructor(app, iconConv),
                  new WpfPropertyManagerPageTextBlockConstructor(app, iconConv),
                  new WpfPropertyManagerPageTabConstructor(app, iconConv),
                  new WpfPropertyManagerPageCustomControlConstructor(app, iconConv),
                  new WpfPropertyManagerPageBitmapButtonConstructor(app, iconConv)
                  )
        {
        }
    }
}

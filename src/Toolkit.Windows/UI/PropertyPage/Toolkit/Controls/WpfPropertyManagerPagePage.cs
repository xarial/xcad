using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.BimVision.UI.PropertyPage.Toolkit.Templates;
using Xarial.XCad.Toolkit.Extensions;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.UI.Wpf;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls
{
    internal class WpfPropertyManagerPagePage : Page, IWpfPropertyManagerPageControlsHost
    {
        internal string Name { get; }
        internal PropertyManagerPageLayout Layout { get; }

        public BitmapImage Icon { get; }

        public string Tooltip { get; }

        public string Label => null;
        public ControlLeftAlign_e Align => ControlLeftAlign_e.LeftEdge;
        public double? Height => null;

        public PageButtons_e Buttons { get; }
        public string Message { get; }
        public string HelpLink { get; }
        public string WhatsNewLink { get; }

        public ObservableCollection<IWpfPropertyManagerPageControl> Controls { get; }

        public override bool Enabled { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override bool Visible { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public DataTemplate Template => throw new NotSupportedException();

        private readonly IHelpLinkHandler m_HelpLinkHandler;

        public WpfPropertyManagerPagePage(IAttributeSet atts, IIconsCreator iconConv, IHelpLinkHandler helpLinkHandler)
        {
            Layout = new PropertyManagerPageLayout();
            Layout.DataContext = this;
            Name = atts.Name;
            Tooltip = atts.Description;

            m_HelpLinkHandler = helpLinkHandler;

            Controls = new ObservableCollection<IWpfPropertyManagerPageControl>();

            Layout.Help += OnHelp;
            Layout.WhatsNew += OnWhatsNew;

            IconAttribute commIconAtt;

            if (atts.ContextType.TryGetAttribute(out commIconAtt))
            {
                if (commIconAtt.Icon != null)
                {
                    Icon = WpfIcon.CreateBitmapImage(commIconAtt.Icon, iconConv);
                }
            }

            if (atts.Has<PageButtonsAttribute>())
            {
                var buttonsAtt = atts.Get<PageButtonsAttribute>();

                Buttons = buttonsAtt.Buttons;
            }
            else
            {
                Buttons = PageButtons_e.Okay | PageButtons_e.Cancel;
            }

            if (atts.Has<HelpAttribute>())
            {
                var helpAtt = atts.Get<HelpAttribute>();

                HelpLink = helpAtt.HelpLink;
                WhatsNewLink = helpAtt.WhatsNewLink;
            }

            if (atts.Has<MessageAttribute>())
            {
                var msgAtt = atts.Get<MessageAttribute>();
                Message = msgAtt.Text;
            }
            else if (!string.IsNullOrEmpty(atts.Description))
            {
                Message = atts.Description;
            }
        }

        private void OnHelp()
        {
            m_HelpLinkHandler.OpenHelpLink(HelpLink);
        }

        private void OnWhatsNew()
        {
            m_HelpLinkHandler.OpenWhatsNewLink(WhatsNewLink);
        }

        public override void ShowTooltip(string title, string msg)
        {
            throw new NotImplementedException();
        }
    }
}

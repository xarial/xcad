//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Blazor.Controls
{
    /// <summary>
    /// Root container of the page
    /// </summary>
    public class BlazorPropertyManagerPagePage : Page, IBlazorPropertyManagerPageControlsHost
    {
        public event Action Invalidated;

        private readonly List<IBlazorPropertyManagerPageControl> m_Controls = new List<IBlazorPropertyManagerPageControl>();
        public IReadOnlyList<IBlazorPropertyManagerPageControl> Controls => m_Controls;

        public string Name { get; }
        public string Tooltip { get; }
        public string Label => null;

        public PageButtons_e Buttons { get; }
        public string Message { get; }
        public string HelpLink { get; }
        public string WhatsNewLink { get; }

        public override bool Enabled { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override bool Visible { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public BlazorPropertyManagerPagePage(IAttributeSet atts)
        {
            Name = atts.Name;
            Tooltip = atts.Description;

            Buttons = atts.Has<PageButtonsAttribute>()
                ? atts.Get<PageButtonsAttribute>().Buttons
                : PageButtons_e.Okay | PageButtons_e.Cancel;

            if (atts.Has<HelpAttribute>())
            {
                var helpAtt = atts.Get<HelpAttribute>();
                HelpLink = helpAtt.HelpLink;
                WhatsNewLink = helpAtt.WhatsNewLink;
            }

            if (atts.Has<MessageAttribute>())
            {
                Message = atts.Get<MessageAttribute>().Text;
            }
            else if (!string.IsNullOrEmpty(atts.Description))
            {
                Message = atts.Description;
            }
        }

        public void AddControl(IBlazorPropertyManagerPageControl control)
        {
            m_Controls.Add(control);
            control.Invalidated += OnChildInvalidated;
            NotifyInvalidated();
        }

        public override void ShowTooltip(string title, string msg)
        {
        }

        /// <summary>
        /// Forces the hosting Razor component to re-render (e.g. after <c>Show(model)</c> pushes new values into controls)
        /// </summary>
        public void NotifyInvalidated() => Invalidated?.Invoke();

        private void OnChildInvalidated() => NotifyInvalidated();
    }
}

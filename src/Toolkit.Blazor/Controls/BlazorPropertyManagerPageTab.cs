//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Blazor.Controls
{
    /// <summary>
    /// Rendered as a tab page within the parent's tab strip
    /// </summary>
    public class BlazorPropertyManagerPageTab : Group, IBlazorPropertyManagerPageControlsHost
    {
        public event Action Invalidated;

        private readonly List<IBlazorPropertyManagerPageControl> m_Controls = new List<IBlazorPropertyManagerPageControl>();
        public IReadOnlyList<IBlazorPropertyManagerPageControl> Controls => m_Controls;

        public string Name { get; }
        public string Header => Name;
        public string Tooltip { get; }
        public string Label => null;

        private bool m_Enabled = true;
        private bool m_Visible = true;

        public BlazorPropertyManagerPageTab(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata)
            : base(atts.Id, atts.Tag, metadata)
        {
            Name = atts.Name;
            Tooltip = atts.Description;

            if (parentGroup is IBlazorPropertyManagerPageControlsHost parentHost)
            {
                parentHost.AddControl(this);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void AddControl(IBlazorPropertyManagerPageControl control)
        {
            m_Controls.Add(control);
            control.Invalidated += OnChildInvalidated;
            NotifyInvalidated();
        }

        public override bool Enabled
        {
            get => m_Enabled;
            set
            {
                m_Enabled = value;
                NotifyInvalidated();
            }
        }

        public override bool Visible
        {
            get => m_Visible;
            set
            {
                m_Visible = value;
                NotifyInvalidated();
            }
        }

        public override void ShowTooltip(string title, string msg)
        {
        }

        private void OnChildInvalidated() => NotifyInvalidated();

        private void NotifyInvalidated() => Invalidated?.Invoke();
    }
}

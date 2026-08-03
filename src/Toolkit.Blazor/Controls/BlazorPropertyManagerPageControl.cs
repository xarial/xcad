//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Blazor.Controls
{
    public abstract class BlazorPropertyManagerPageControl<TVal> : Control<TVal>, IBlazorPropertyManagerPageControl
    {
        /// <inheritdoc/>
        public event Action Invalidated;

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string Tooltip { get; }

        /// <inheritdoc/>
        public string Label { get; }

        private bool m_Enabled;
        private bool m_Visible;

        protected BlazorPropertyManagerPageControl(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata)
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

            var opts = GetControlOptions(atts);

            m_Visible = opts.Options.HasFlag(AddControlOptions_e.Visible);
            m_Enabled = opts.Options.HasFlag(AddControlOptions_e.Enabled);

            if (atts.Has<LabelAttribute>())
            {
                Label = atts.Get<LabelAttribute>().Caption;
            }
        }

        /// <inheritdoc/>
        public override bool Enabled
        {
            get => m_Enabled;
            set
            {
                m_Enabled = value;
                NotifyInvalidated();
            }
        }

        /// <inheritdoc/>
        public override bool Visible
        {
            get => m_Visible;
            set
            {
                m_Visible = value;
                NotifyInvalidated();
            }
        }

        /// <inheritdoc/>
        public override void Focus()
        {
        }

        /// <inheritdoc/>
        public override void ShowTooltip(string title, string msg)
        {
        }

        /// <summary>
        /// Notifies the hosting component that this control needs to be re-rendered
        /// </summary>
        protected internal void NotifyInvalidated() => Invalidated?.Invoke();

        private static IControlOptionsAttribute GetControlOptions(IAttributeSet atts)
            => atts.Has<ControlOptionsAttribute>() ? atts.Get<ControlOptionsAttribute>() : new ControlOptionsAttribute();
    }
}

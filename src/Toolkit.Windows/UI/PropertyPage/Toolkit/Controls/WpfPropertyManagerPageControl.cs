//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls
{
    public interface IWpfPropertyManagerPageControl : IControl 
    {
        string Tooltip { get; }
        BitmapImage Icon { get; }
        string Label { get; }
        ControlLeftAlign_e Align { get; }
        DataTemplate Template { get; }
        double? Width { get; }
        double? Height { get; }
        double? Top { get; }
        double? Left { get; }
    }

    internal abstract class WpfPropertyManagerPageControl<TVal> : Control<TVal>, IWpfPropertyManagerPageControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; }
        public string Tooltip { get; }

        public BitmapImage Icon { get; }
        public string Label { get; }

        public ControlLeftAlign_e Align { get; }

        public virtual double? Width { get; }
        public virtual double? Height { get; }
        public virtual double? Top { get; }
        public virtual double? Left { get; }

        private bool m_Enabled;
        private bool m_Visible;

        public WpfPropertyManagerPageControl(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, IIconsCreator iconsConv) : base(atts.Id, atts.Tag, metadata)
        {
            Name = atts.Name;
            Tooltip = atts.Description;

            if (parentGroup is IWpfPropertyManagerPageControlsHost parentHost)
            {
                parentHost.Controls.Add(this);
            }
            else
            {
                throw new NotSupportedException();
            }

            var opts = GetControlOptions(atts);

            InitData(opts, atts);

            m_Visible = opts.Options.HasFlag(AddControlOptions_e.Visible);
            m_Enabled = opts.Options.HasFlag(AddControlOptions_e.Enabled);

            Align = opts.Align;

            if (atts.Has<LabelAttribute>())
            {
                var labelAtt = atts.Get<LabelAttribute>();

                Label = labelAtt.Caption;
            }

            if (opts.Height > 0)
            {
                Height = Convert.ToDouble(opts.Height);
            }
            else 
            {
                Height = null;
            }

            if (opts.Width > 0)
            {
                Width = Convert.ToDouble(opts.Width);
            }
            else
            {
                Width = null;
            }

            if (opts.Left != -1)
            {
                Left = Convert.ToDouble(opts.Left);
            }
            else
            {
                Left = null;
            }

            if (opts.Top != -1)
            {
                Top = Convert.ToDouble(opts.Top);
            }
            else
            {
                Top = null;
            }

            var commonIcon = atts.ControlDescriptor?.Icon;

            if (commonIcon != null)
            {
                Icon = WpfIcon.CreateBitmapImage(commonIcon, iconsConv);
            }

            if (atts.Has<StandardControlIconAttribute>()) 
            {
                //TODO: implement standard icon
            }
        }

        public override bool Enabled 
        {
            get => m_Enabled;
            set
            {
                m_Enabled = value;
                NotifyPropertyChanged();
            }
        }

        public override bool Visible
        {
            get => m_Visible;
            set
            {
                m_Visible = value;
                NotifyPropertyChanged();
            }
        }

        public abstract DataTemplate Template { get; }

        public override void Focus()
        {
            throw new NotImplementedException();
        }

        public override void ShowTooltip(string title, string msg)
        {
            throw new NotImplementedException();
        }

        protected void NotifyPropertyChanged([CallerMemberName]string prpName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prpName));
        }

        protected virtual void InitData(IControlOptionsAttribute opts, IAttributeSet atts)
        {
        }

        private IControlOptionsAttribute GetControlOptions(IAttributeSet atts)
        {
            ControlOptionsAttribute opts;

            if (atts.Has<ControlOptionsAttribute>())
            {
                opts = atts.Get<ControlOptionsAttribute>();
            }
            else
            {
                opts = new ControlOptionsAttribute();
            }

            return opts;
        }
    }

    internal static class WpfGroupExtension
    {
        internal static WpfPropertyManagerPagePage FindParentPage(this IGroup thisGroup) 
        {
            switch (thisGroup)
            {
                case WpfPropertyManagerPagePage page:
                    return page;

                case WpfPropertyManagerPageGroup group:
                    return group.ParentPage;

                case WpfPropertyManagerPageTab tab:
                    return tab.ParentPage;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}

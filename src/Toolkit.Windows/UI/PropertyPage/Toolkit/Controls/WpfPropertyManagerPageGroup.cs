using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Xarial.XCad.BimVision.UI.PropertyPage.Toolkit.Templates;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Templates;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls
{
    internal class WpfPropertyManagerPageGroup : Group, IWpfPropertyManagerPageControlsHost, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<IWpfPropertyManagerPageControl> Controls { get; }

        public DataTemplate Template { get; }

        public string Header { get; }

        public bool IsExpanded { get; }

        public string Tooltip { get; }

        public BitmapImage Icon => null;
        public string Label => null;
        public ControlLeftAlign_e Align => ControlLeftAlign_e.LeftEdge;
        public double? Height => null;

        private bool m_Enabled;
        private bool m_Visible;

        public WpfPropertyManagerPageGroup(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata) : base(atts.Id, atts.Tag, metadata)
        {
            m_Visible = true;
            m_Enabled = true;

            Controls = new ObservableCollection<IWpfPropertyManagerPageControl>();

            if (parentGroup is IWpfPropertyManagerPageControlsHost parentHost)
            {
                parentHost.Controls.Add(this);
            }
            else 
            {
                throw new NotSupportedException();
            }

            Header = atts.Name;

            GroupBoxOptions_e opts = 0;

            if (atts.Has<IGroupBoxOptionsAttribute>())
            {
                opts = atts.Get<IGroupBoxOptionsAttribute>().Options;
            }

            IsExpanded = !opts.HasFlag(GroupBoxOptions_e.Collapsed);

            Tooltip = atts.Description;

            Template = ControlTemplates.Group;
        }

        public override bool Enabled
        {
            get => m_Enabled;
            set
            {
                m_Enabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Enabled)));
            }
        }

        public override bool Visible
        {
            get => m_Visible;
            set
            {
                m_Visible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Visible)));
            }
        }

        public override void ShowTooltip(string title, string msg)
        {
            throw new NotImplementedException();
        }
    }
}

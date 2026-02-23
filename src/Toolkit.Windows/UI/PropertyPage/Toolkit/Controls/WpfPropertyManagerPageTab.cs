//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Templates;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls
{
    internal class WpfPropertyManagerPageTab : Group, IWpfPropertyManagerPageControlsHost, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<IWpfPropertyManagerPageControl> Controls { get; }

        public DataTemplate Template { get; }

        public string Header { get; }

        public string Tooltip { get; }

        public BitmapImage Icon => null;
        public string Label => null;
        public ControlLeftAlign_e Align => ControlLeftAlign_e.LeftEdge;
        
        public double? Width => null;
        public double? Height => null;
        public double? Top => null;
        public double? Left => null;

        internal WpfPropertyManagerPagePage ParentPage { get; }

        private bool m_Enabled;
        private bool m_Visible;

        public WpfPropertyManagerPageTab(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata) : base(atts.Id, atts.Tag, metadata)
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

            ParentPage = parentGroup.FindParentPage();

            Header = atts.Name;

            Tooltip = atts.Description;

            Template = ControlTemplates.Tab;
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
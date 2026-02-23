//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Templates;
using Xarial.XCad.Toolkit.Windows.UI.Wpf;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls
{
    internal class WpfPropertyManagerPageBitmapButton : WpfPropertyManagerPageControl<object>
    {
        private enum Mode_e 
        {
            Toggle,
            Button
        }

        private const double DEFAULT_SIZE = 24;

        public bool Checked
        {
            get => m_Checked.Value;
            set
            {
                m_Checked = value;
                this.NotifyPropertyChanged();

                if (m_ToggleOffImage != null) 
                {
                    this.NotifyPropertyChanged(nameof(Image));
                }

                this.ValueChanged?.Invoke(this, value);
            }
        }

        public ICommand ClickCommand { get; }

        public override double? Width 
        {
            get 
            {
                var width = base.Width;

                if (width.HasValue)
                {
                    return width.Value;
                }
                else 
                {
                    return DEFAULT_SIZE;
                }
            }
        }

        public override double? Height
        {
            get
            {
                var height = base.Height;

                if (height.HasValue)
                {
                    return height.Value;
                }
                else
                {
                    return DEFAULT_SIZE;
                }
            }
        }

        public BitmapImage Image
        {
            get
            {
                if (m_Mode == Mode_e.Toggle && m_ToggleOffImage != null && !Checked)
                {
                    return m_ToggleOffImage;
                }
                else
                {
                    return m_Image;
                }
            }
        }

        public override DataTemplate Template { get; }

        protected override event ControlValueChangedDelegate<object> ValueChanged;

        private bool? m_Checked;

        private Action m_ButtonClickHandler;

        private BitmapImage m_Image;

        private BitmapImage m_ToggleOffImage;

        private readonly Mode_e m_Mode;

        public WpfPropertyManagerPageBitmapButton(IGroup parentGroup, IIconsCreator iconsConv,
            IAttributeSet atts, IMetadata[] metadata)
            : base(parentGroup, atts, metadata, iconsConv)
        {
            if (atts.ContextType == typeof(bool))
            {
                m_Mode = Mode_e.Toggle;

                m_Checked = false;

                Template = ControlTemplates.BitmapToggleButton;
            }
            else if (atts.ContextType == typeof(Action))
            {
                m_Mode = Mode_e.Button;

                ClickCommand = new RelayCommand(OnClick);

                Template = ControlTemplates.BitmapButton;
            }
            else
            {
                throw new NotSupportedException();
            }

            var bmpAtt = atts.Get<BitmapButtonAttribute>();

            if (bmpAtt.StandardIcon.HasValue)
            {
                //TODO: implement
            }
            else
            {
                var icon = bmpAtt.Icon ?? Defaults.Icon;

                m_Image = WpfIcon.CreateBitmapImage(icon, iconsConv);

                if (bmpAtt is BitmapToggleButtonAttribute)
                {
                    var toggledIcon = ((BitmapToggleButtonAttribute)bmpAtt).ToggledOffIcon;

                    if (toggledIcon != null)
                    {
                        m_ToggleOffImage = WpfIcon.CreateBitmapImage(toggledIcon, iconsConv, ((BitmapToggleButtonAttribute)bmpAtt).ToggledOffEffect);
                    }
                }
            }
        }

        private void OnClick()
        {
            m_ButtonClickHandler.Invoke();
        }

        protected override object GetSpecificValue()
        {
            switch (m_Mode) 
            {
                case Mode_e.Toggle:
                    return Checked;

                case Mode_e.Button:
                    return m_ButtonClickHandler;

                default:
                    throw new NotSupportedException();
            }
        }

        protected override void SetSpecificValue(object value)
        {
            switch (m_Mode)
            {
                case Mode_e.Toggle:
                    Checked = (bool)value;
                    break;

                case Mode_e.Button:
                    m_ButtonClickHandler = (Action)value;
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                base.Dispose(disposing);
            }
        }
    }
}
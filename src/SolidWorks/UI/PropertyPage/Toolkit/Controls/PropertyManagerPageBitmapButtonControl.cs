//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Linq;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal class PropertyManagerPageBitmapButtonControl : PropertyManagerPageBaseControl<object, IPropertyManagerPageBitmapButton>
    {
        protected override event ControlValueChangedDelegate<object> ValueChanged;

        private Action m_ButtonClickHandler;

        private swPropertyManagerPageControlType_e m_Type;

        private IImageCollection m_Bitmap;
        private IImageCollection m_ToggledOffBitmap;

        public PropertyManagerPageBitmapButtonControl(SwApplication app, IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            : base(app, parentGroup, iconConv, atts, metadata, (swPropertyManagerPageControlType_e)(-1), ref numberOfUsedIds)
        {
            m_Handler.ButtonPressed += OnButtonPressed;
        }

        protected override void InitData(IControlOptionsAttribute opts, IAttributeSet atts)
        {
            m_Type = GetButtonSpecificType(atts);
        }

        protected override IPropertyManagerPageBitmapButton Create(IGroup host, int id, string name, ControlLeftAlign_e align,
            AddControlOptions_e options, string description, swPropertyManagerPageControlType_e type)
            => CreateSwControl<IPropertyManagerPageBitmapButton>(host, id, name, align, options, description, m_Type);

        private string[] m_ImgList;
        private string[] m_MaskImgList;

        private string[] m_ImgListToggledOff;
        private string[] m_MaskImgListToggledOff;

        private string m_LegacyIcon;
        private string m_MaskLegacyIcon;

        private string m_LegacyIconToggledOff;
        private string m_MaskLegacyIconToggledOff;

        protected override void SetOptions(IPropertyManagerPageBitmapButton ctrl, IControlOptionsAttribute opts, IAttributeSet atts)
        {
            var bmpAtt = atts.Get<BitmapButtonAttribute>();

            if (bmpAtt.StandardIcon.HasValue)
            {
                ctrl.SetStandardBitmaps((int)bmpAtt.StandardIcon.Value);
            }
            else
            {
                var bmpWidth = bmpAtt.Width;
                var bmpHeight = bmpAtt.Height;

                var icon = bmpAtt.Icon ?? Defaults.Icon;

                if (m_App.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2016))
                {
                    m_Bitmap = m_IconConv.ConvertIcon(new BitmapButtonHighResIcon(icon, bmpWidth, bmpHeight));
                    var icons = m_Bitmap.FilePaths;

                    m_ImgList = icons.Take(6).ToArray();
                    m_MaskImgList = icons.Skip(6).ToArray();

                    var defImgList = m_ImgList;
                    var defImgListMask = m_MaskImgList;

                    if (bmpAtt is BitmapToggleButtonAttribute)
                    {
                        var toggledIcon = ((BitmapToggleButtonAttribute)bmpAtt).ToggledOffIcon;

                        if (toggledIcon != null)
                        {
                            m_ToggledOffBitmap = m_IconConv.ConvertIcon(new BitmapButtonHighResIcon(toggledIcon, bmpWidth, bmpHeight,
                                ((BitmapToggleButtonAttribute)bmpAtt).ToggledOffEffect));
                            var toggledIcons = m_ToggledOffBitmap.FilePaths;

                            m_ImgListToggledOff = toggledIcons.Take(6).ToArray();
                            m_MaskImgListToggledOff = toggledIcons.Skip(6).ToArray();

                            //button is always toggled off by default and will be overriden via SetValue
                            defImgList = m_ImgListToggledOff;
                            defImgListMask = m_MaskImgListToggledOff;
                        }
                    }

                    ctrl.SetBitmapsByName3(defImgList, defImgListMask);
                }
                else
                {
                    m_Bitmap = m_IconConv.ConvertIcon(new BitmapButtonIcon(icon, bmpWidth, bmpHeight));
                    var icons = m_Bitmap.FilePaths;
                    
                    m_LegacyIcon = icons[0];
                    m_MaskLegacyIcon = icons[1];

                    var defIcon = m_LegacyIcon;
                    var defMaskIcon = m_MaskLegacyIcon;

                    if (bmpAtt is BitmapToggleButtonAttribute)
                    {
                        var toggledOffIcon = ((BitmapToggleButtonAttribute)bmpAtt).ToggledOffIcon;

                        m_ToggledOffBitmap = m_IconConv.ConvertIcon(new BitmapButtonIcon(toggledOffIcon, bmpWidth, bmpHeight,
                            ((BitmapToggleButtonAttribute)bmpAtt).ToggledOffEffect));
                        var toggledIcons = m_ToggledOffBitmap.FilePaths;
                        
                        m_LegacyIconToggledOff = toggledIcons[0];
                        m_MaskLegacyIconToggledOff = toggledIcons[1];

                        //button is always toggled off by default and will be overriden via SetValue
                        defIcon = m_LegacyIconToggledOff;
                        defMaskIcon = m_MaskLegacyIconToggledOff;
                    }
                    
                    ctrl.SetBitmapsByName2(defIcon, defMaskIcon);
                }
            }
        }

        private swPropertyManagerPageControlType_e GetButtonSpecificType(IAttributeSet atts)
        {
            if (atts.ContextType == typeof(bool))
            {
                return swPropertyManagerPageControlType_e.swControlType_CheckableBitmapButton;
            }
            else if (atts.ContextType == typeof(Action))
            {
                return swPropertyManagerPageControlType_e.swControlType_BitmapButton;
            }
            else
            {
                throw new NotSupportedException("Only bool and Action types are supported for bitmap button");
            }
        }

        private void OnButtonPressed(int id)
        {
            if (Id == id)
            {
                if (SwSpecificControl.IsCheckable)
                {
                    UpdateIconIfNeeded();

                    ValueChanged?.Invoke(this, SwSpecificControl.Checked);
                }
                else
                {
                    if (m_ButtonClickHandler == null)
                    {
                        throw new NullReferenceException("Button click handler is not specified. Set the value of the delegate to the handler method");
                    }

                    m_ButtonClickHandler.Invoke();
                }
            }
        }

        protected override object GetSpecificValue()
        {
            if (SwSpecificControl.IsCheckable)
            {
                return SwSpecificControl.Checked;
            }
            else
            {
                return m_ButtonClickHandler;
            }
        }

        protected override void SetSpecificValue(object value)
        {
            if (SwSpecificControl.IsCheckable)
            {
                SwSpecificControl.Checked = (bool)value;

                UpdateIconIfNeeded();
            }
            else 
            {
                m_ButtonClickHandler = (Action)value;
            }
        }

        private void UpdateIconIfNeeded()
        {
            if (m_ToggledOffBitmap != null)
            {
                if (m_App.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2016))
                {
                    if (SwSpecificControl.Checked)
                    {
                        SwSpecificControl.SetBitmapsByName3(m_ImgList, m_MaskImgList);
                    }
                    else 
                    {
                        SwSpecificControl.SetBitmapsByName3(m_ImgListToggledOff, m_MaskImgListToggledOff);
                    }
                }
                else
                {
                    if (SwSpecificControl.Checked)
                    {
                        SwSpecificControl.SetBitmapsByName2(m_LegacyIcon, m_MaskLegacyIcon);
                    }
                    else 
                    {
                        SwSpecificControl.SetBitmapsByName2(m_LegacyIconToggledOff, m_MaskLegacyIconToggledOff);
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                base.Dispose(disposing);

                m_Handler.ButtonPressed -= OnButtonPressed;

                m_Bitmap?.Dispose();
                m_ToggledOffBitmap?.Dispose();
            }
        }
    }
}
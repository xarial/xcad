//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Linq;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons;
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

                    var imgList = icons.Take(6).ToArray();
                    var maskImgList = icons.Skip(6).ToArray();
                    ctrl.SetBitmapsByName3(imgList, maskImgList);
                }
                else
                {
                    m_Bitmap = m_IconConv.ConvertIcon(new BitmapButtonIcon(icon, bmpWidth, bmpHeight));
                    var icons = m_Bitmap.FilePaths;

                    ctrl.SetBitmapsByName2(icons[0], icons[1]);
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
            }
            else 
            {
                m_ButtonClickHandler = (Action)value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                base.Dispose(disposing);

                m_Handler.ButtonPressed -= OnButtonPressed;

                m_Bitmap?.Dispose();
            }
        }
    }
}
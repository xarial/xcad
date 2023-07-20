//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Drawing;
using Xarial.XCad.Enums;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Utils;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal static class PropertyManagerPageLabelExtension 
    {
        internal static void SetLabelOptions(this IPropertyManagerPageLabel label, FontStyle_e style, string font, KnownColor? textColor) 
        {
            if (style.HasFlag(FontStyle_e.Bold))
            {
                label.Bold[0, (short)(label.Caption.Length - 1)] = true;
            }

            if (style.HasFlag(FontStyle_e.Italic))
            {
                label.Italic[0, (short)(label.Caption.Length - 1)] = true;
            }

            if (style.HasFlag(FontStyle_e.Underline))
            {
                label.Underline[0, (short)(label.Caption.Length - 1)] = (int)swPropMgrPageLabelUnderlineStyle_e.swPropMgrPageLabel_SolidUnderline;
            }

            if (!string.IsNullOrEmpty(font))
            {
                label.Font[0, (short)(label.Caption.Length - 1)] = font;
            }

            if (textColor.HasValue) 
            {
                label.CharacterColor[0, (short)(label.Caption.Length - 1)] = ColorUtils.ToColorRef(Color.FromKnownColor(textColor.Value));
            }
        }
    }

    internal class PropertyManagerPageTextBlockControl : PropertyManagerPageBaseControl<object, IPropertyManagerPageLabel>
    {
        protected override event ControlValueChangedDelegate<object> ValueChanged;

        private FontStyle_e m_FontStyle;
        private string m_Font;
        private KnownColor? m_TextColor;
        private string m_Format;

        internal PropertyManagerPageTextBlockControl(SwApplication app, IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            : base(app, parentGroup, iconConv, atts, metadata, swPropertyManagerPageControlType_e.swControlType_Label, ref numberOfUsedIds)
        {
        }

        protected override void InitData(IControlOptionsAttribute opts, IAttributeSet atts)
        {
            if (atts.Has<ControlOptionsAttribute>())
            {
                m_TextColor = atts.Get<ControlOptionsAttribute>().TextColor;
            }

            if (atts.Has<TextBlockOptionsAttribute>())
            {
                var style = atts.Get<TextBlockOptionsAttribute>();
                
                m_FontStyle = style.FontStyle;

                m_Font = style.Font;

                m_Format = style.Format;
            }
        }

        protected override void SetOptions(IPropertyManagerPageLabel ctrl, IControlOptionsAttribute opts, IAttributeSet atts)
        {
            if (atts.Has<TextBlockOptionsAttribute>())
            {
                var style = atts.Get<TextBlockOptionsAttribute>();

                ctrl.Style = (int)style.TextAlignment;

                ctrl.SetLabelOptions(m_FontStyle, m_Font, m_TextColor);
            }
        }

        protected override object GetSpecificValue() => null;

        protected override void SetSpecificValue(object value)
        {
            string caption;

            if (string.IsNullOrEmpty(m_Format))
            {
                caption = value?.ToString();
            }
            else 
            {
                caption = string.Format(m_Format, value);
            }

            SwSpecificControl.Caption = caption;
            SwSpecificControl.SetLabelOptions(m_FontStyle, m_Font, m_TextColor);
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}
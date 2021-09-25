//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Drawing;
using Xarial.XCad.Toolkit.Utils;
using Xarial.XCad.UI.PropertyPage.Enums;
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

    internal class PropertyManagerPageTextBlockControl : PropertyManagerPageBaseControl<string, IPropertyManagerPageLabel>
    {
        protected override event ControlValueChangedDelegate<string> ValueChanged;

        private readonly FontStyle_e m_FontStyle;
        private readonly string m_Font;
        private readonly KnownColor? m_TextColor;

        internal PropertyManagerPageTextBlockControl(int id, object tag,
            IPropertyManagerPageLabel textBlock, FontStyle_e fontStyle, string font, KnownColor? textColor,
            SwPropertyManagerPageHandler handler, IPropertyManagerPageLabel label) : base(textBlock, id, tag, handler, label)
        {
            m_FontStyle = fontStyle;
            m_Font = font;
            m_TextColor = textColor;
        }

        protected override string GetSpecificValue()
            => SwSpecificControl.Caption;

        protected override void SetSpecificValue(string value)
        {
            SwSpecificControl.Caption = value;
            SwSpecificControl.SetLabelOptions(m_FontStyle, m_Font, m_TextColor);
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}
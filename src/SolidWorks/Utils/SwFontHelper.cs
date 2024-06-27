//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.Enums;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal class SwTextFormat : IFont
    {
        internal ITextFormat TextFormat { get; }

        public string Name
        {
            get => TextFormat.TypeFaceName;
            set => TextFormat.TypeFaceName = value;
        }

        public double? Size 
        {
            get 
            {
                if (TextFormat.IsHeightSpecifiedInPts())
                {
                    return null;
                }
                else
                {
                    return TextFormat.CharHeight;
                }
            }
            set 
            {
                if (value.HasValue)
                {
                    TextFormat.CharHeight = value.Value;
                }
                else 
                {
                    throw new Exception($"Use '{nameof(SizeInPoints)}' to specify size in points");
                }
            }
        }

        public int? SizeInPoints
        {
            get
            {
                if (TextFormat.IsHeightSpecifiedInPts())
                {
                    return TextFormat.CharHeightInPts;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.HasValue)
                {
                    TextFormat.CharHeightInPts = value.Value;
                }
                else
                {
                    throw new Exception($"Use '{nameof(Size)}' to specify size in points");
                }
            }
        }

        public FontStyle_e Style 
        {
            get 
            {
                var style = FontStyle_e.Regular;

                if (TextFormat.Bold)
                {
                    style |= FontStyle_e.Bold;
                }

                if (TextFormat.Italic)
                {
                    style |= FontStyle_e.Italic;
                }

                if (TextFormat.Strikeout)
                {
                    style |= FontStyle_e.Strikeout;
                }

                if (TextFormat.Underline)
                {
                    style |= FontStyle_e.Underline;
                }

                return style;
            }
            set
            {
                TextFormat.Bold = value.HasFlag(FontStyle_e.Bold);
                TextFormat.Italic = value.HasFlag(FontStyle_e.Italic);
                TextFormat.Underline = value.HasFlag(FontStyle_e.Underline);
                TextFormat.Strikeout = value.HasFlag(FontStyle_e.Strikeout);
            }
        }

        internal SwTextFormat(ITextFormat txtFormat) 
        {
            if (txtFormat == null)
            {
                throw new ArgumentNullException(nameof(txtFormat));
            }

            TextFormat = txtFormat;
        }
    }

    internal class SwFontHelper
    {
        internal static void FillTextFormat(IFont font, ITextFormat txtFormat) 
        {
            if (font == null) 
            {
                throw new ArgumentNullException(nameof(font));
            }

            var swTextFormat = new SwTextFormat(txtFormat);

            swTextFormat.Name = font.Name;
            
            if (font.Size.HasValue)
            {
                swTextFormat.Size = font.Size;
            }

            if (font.SizeInPoints.HasValue) 
            {
                swTextFormat.SizeInPoints = font.SizeInPoints;
            }

            swTextFormat.Style = font.Style;
        }
    }
}

//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Enums;
using Xarial.XCad.Toolkit;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal class SwFontHelper
    {
        internal static IFont FromTextFormat(ITextFormat txtFormat)
        {
            if (txtFormat == null) 
            {
                throw new ArgumentNullException(nameof(txtFormat));
            }

            var style = FontStyle_e.Regular;

            if (txtFormat.Bold) 
            {
                style |= FontStyle_e.Bold;
            }

            if (txtFormat.Italic) 
            {
                style |= FontStyle_e.Italic;
            }

            if (txtFormat.Strikeout) 
            {
                style |= FontStyle_e.Strikeout;
            }

            if (txtFormat.Underline)
            {
                style |= FontStyle_e.Underline;
            }

            double? height;
            double? heightInPts;

            if (txtFormat.IsHeightSpecifiedInPts())
            {
                heightInPts = txtFormat.CharHeightInPts;
                height = null;
            }
            else 
            {
                heightInPts = null;
                height = txtFormat.CharHeight;
            }

            return new Font(txtFormat.TypeFaceName, height, heightInPts, style);
        }

        internal static void FillTextFormat(IFont font, ITextFormat txtFormat) 
        {
            if (font == null) 
            {
                throw new ArgumentNullException(nameof(font));
            }

            if (txtFormat == null)
            {
                throw new ArgumentNullException(nameof(txtFormat));
            }
            
            txtFormat.TypeFaceName = font.Name;

            if (font.Size.HasValue) 
            {
                txtFormat.CharHeight = font.Size.Value;
            }
            else if (font.SizeInPoints.HasValue)
            {
                txtFormat.CharHeightInPts = Convert.ToInt32(font.SizeInPoints.Value);
            }

            txtFormat.Bold = font.Style.HasFlag(FontStyle_e.Bold);
            txtFormat.Italic = font.Style.HasFlag(FontStyle_e.Italic);
            txtFormat.Underline = font.Style.HasFlag(FontStyle_e.Underline);
            txtFormat.Strikeout = font.Style.HasFlag(FontStyle_e.Strikeout);
        }
    }
}

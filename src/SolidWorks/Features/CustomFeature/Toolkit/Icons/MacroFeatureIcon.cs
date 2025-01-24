//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using Xarial.XCad.SolidWorks.Base;
using Xarial.XCad.Toolkit.Base;
using Xarial.XCad.Toolkit.Utils;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.Features.CustomFeature.Toolkit.Icons
{
    internal class MacroFeatureIcon : IIcon
    {
        protected readonly string m_BaseName;
        protected readonly IXImage m_Icon;

        public Color TransparencyKey => Color.White;

        public bool IsPermanent => true;

        public IconImageFormat_e Format => IconImageFormat_e.Bmp;

        internal MacroFeatureIcon(IXImage icon, string baseName)
        {
            m_BaseName = baseName;
            m_Icon = icon;

            IconSizes = new IIconSpec[]
            {
                new IconSpec(m_Icon, MacroFeatureIconInfo.Size, 0, m_BaseName)
            };
        }

        public virtual IIconSpec[] IconSizes { get; }
    }

    internal class MacroFeatureSuppressedIcon : MacroFeatureIcon
    {
        internal MacroFeatureSuppressedIcon(IXImage icon, string baseName) : base(icon, baseName)
        {
            IconSizes = new IIconSpec[]
            {
                new IconSpec(m_Icon, MacroFeatureIconInfo.Size, ConvertPixelToGrayscale, 0, m_BaseName)
            };
        }

        public override IIconSpec[] IconSizes { get; }

        protected void ConvertPixelToGrayscale(ref byte r, ref byte g, ref byte b, ref byte a)
            => ColorUtils.ConvertPixelToGrayscale(ref r, ref g, ref b);
    }
}
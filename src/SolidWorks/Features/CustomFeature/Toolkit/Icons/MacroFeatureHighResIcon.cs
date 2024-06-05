﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using System.Drawing;
using Xarial.XCad.SolidWorks.Base;
using Xarial.XCad.Toolkit.Base;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.Features.CustomFeature.Toolkit.Icons
{
    internal class MacroFeatureHighResIcon : MacroFeatureIcon
    {
        internal MacroFeatureHighResIcon(IXImage icon, string baseName) : base(icon, baseName)
        {
            IconSizes =  new IIconSpec[]
            {
                new IconSpec(m_Icon, MacroFeatureIconInfo.SizeHighResSmall, 0, m_BaseName),
                new IconSpec(m_Icon, MacroFeatureIconInfo.SizeHighResMedium, 0, m_BaseName),
                new IconSpec(m_Icon, MacroFeatureIconInfo.SizeHighResLarge, 0, m_BaseName)
            };
        }

        public override IIconSpec[] IconSizes { get; }
    }

    internal class MacroFeatureSuppressedHighResIcon : MacroFeatureSuppressedIcon
    {
        internal MacroFeatureSuppressedHighResIcon(IXImage icon, string baseName) : base(icon, baseName)
        {
            IconSizes = new IIconSpec[]
            {
                new IconSpec(m_Icon, MacroFeatureIconInfo.SizeHighResSmall, ConvertPixelToGrayscale, 0, m_BaseName),
                new IconSpec(m_Icon, MacroFeatureIconInfo.SizeHighResMedium, ConvertPixelToGrayscale, 0, m_BaseName),
                new IconSpec(m_Icon, MacroFeatureIconInfo.SizeHighResLarge, ConvertPixelToGrayscale, 0, m_BaseName)
            };
        }

        public override IIconSpec[] IconSizes { get; }
    }
}
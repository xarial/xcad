//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Drawing;
using System.IO;
using Xarial.XCad.SolidWorks.Base;
using Xarial.XCad.Toolkit.Base;

namespace Xarial.XCad.SolidWorks.Features.CustomFeature.Toolkit
{
    internal static class MacroFeatureIconInfo
    {
        internal const string DEFAULT_ICON_FOLDER = "Xarial\\xCad.Sw\\{0}\\Icons";
        internal const string RegularName = "Regular";
        internal const string SuppressedName = "Suppressed";
        internal const string HighlightedName = "Highlighted";

        internal static Size Size { get; private set; } = new Size(16, 18);
        internal static Size SizeHighResSmall { get; private set; } = new Size(20, 20);
        internal static Size SizeHighResMedium { get; private set; } = new Size(32, 32);
        internal static Size SizeHighResLarge { get; private set; } = new Size(40, 40);

        internal static string GetLocation(Type macroFeatType)
        {
            var iconFolderName = "";

            //macroFeatType.TryGetAttribute<FeatureIconAttribute>(a => iconFolderName = a.IconFolderName);

            if (string.IsNullOrEmpty(iconFolderName))
            {
                iconFolderName = string.Format(DEFAULT_ICON_FOLDER, macroFeatType.FullName);
            }

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                iconFolderName);
        }

        internal static string[] GetIcons(Type macroFeatType, bool highRes)
        {
            var loc = GetLocation(macroFeatType);

            if (highRes)
            {
                return new string[]
                {
                    Path.Combine(loc, IconSpec.CreateFileName(RegularName, SizeHighResSmall)),
                    Path.Combine(loc, IconSpec.CreateFileName(SuppressedName, SizeHighResSmall)),
                    Path.Combine(loc, IconSpec.CreateFileName(HighlightedName, SizeHighResSmall)),
                    Path.Combine(loc, IconSpec.CreateFileName(RegularName, SizeHighResMedium)),
                    Path.Combine(loc, IconSpec.CreateFileName(SuppressedName, SizeHighResMedium)),
                    Path.Combine(loc, IconSpec.CreateFileName(HighlightedName, SizeHighResMedium)),
                    Path.Combine(loc, IconSpec.CreateFileName(RegularName, SizeHighResLarge)),
                    Path.Combine(loc, IconSpec.CreateFileName(SuppressedName, SizeHighResLarge)),
                    Path.Combine(loc, IconSpec.CreateFileName(HighlightedName, SizeHighResLarge))
                };
            }
            else
            {
                return new string[]
                {
                    Path.Combine(loc, IconSpec.CreateFileName(RegularName, Size)),
                    Path.Combine(loc, IconSpec.CreateFileName(SuppressedName, Size)),
                    Path.Combine(loc, IconSpec.CreateFileName(HighlightedName, Size))
                };
            }
        }
    }
}
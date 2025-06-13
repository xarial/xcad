//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Xarial.XCad.Enums;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal static class SwColorHelper
    {
        private static readonly IReadOnlyDictionary<SystemColor_e, swUserPreferenceIntegerValue_e> m_SystemColorToUserPrefMap;
        private static readonly IReadOnlyDictionary<swUserPreferenceIntegerValue_e, SystemColor_e> m_UserPrefToSystemColorMap;

        static SwColorHelper() 
        {
            m_SystemColorToUserPrefMap = new Dictionary<SystemColor_e, swUserPreferenceIntegerValue_e>()
            {
                { SystemColor_e.SelectedItem1, swUserPreferenceIntegerValue_e.swSystemColorsSelectedItem1 },
                { SystemColor_e.SelectedItem2, swUserPreferenceIntegerValue_e.swSystemColorsSelectedItem2 },
                { SystemColor_e.SelectedItem3, swUserPreferenceIntegerValue_e.swSystemColorsSelectedItem3 },
                { SystemColor_e.SelectedItem4, swUserPreferenceIntegerValue_e.swSystemColorsSelectedItem4 },
                { SystemColor_e.SelectedItem5, swUserPreferenceIntegerValue_e.swSystemColorsSelectedItem5 },
                { SystemColor_e.SelectedItem6, swUserPreferenceIntegerValue_e.swSystemColorsSelectedItem6 },
                { SystemColor_e.SelectedItemMissingReference, swUserPreferenceIntegerValue_e.swSystemColorsGhostSelColor },
                { SystemColor_e.Highlight, swUserPreferenceIntegerValue_e.swSystemColorsHighlight },
                { SystemColor_e.InactiveEntity, swUserPreferenceIntegerValue_e.swSystemColorsInactiveEntity },
                { SystemColor_e.TemporaryGraphics, swUserPreferenceIntegerValue_e.swSystemColorsTemporaryGraphics },
                { SystemColor_e.TemporaryGraphicsShaded, swUserPreferenceIntegerValue_e.swSystemColorsTemporaryGraphicsShaded },
                { SystemColor_e.SketchOverDefined, swUserPreferenceIntegerValue_e.swSystemColorsSketchOverDefined },
                { SystemColor_e.SketchFullyDefined, swUserPreferenceIntegerValue_e.swSystemColorsSketchFullyDefined },
                { SystemColor_e.SketchUnderDefined, swUserPreferenceIntegerValue_e.swSystemColorsSketchUnderDefined },
                { SystemColor_e.SketchInvalidGeometry, swUserPreferenceIntegerValue_e.swSystemColorsSketchInvalidGeometry },
                { SystemColor_e.SketchNotSolved, swUserPreferenceIntegerValue_e.swSystemColorsSketchNotSolved },
                { SystemColor_e.SketchInactive, swUserPreferenceIntegerValue_e.swSystemColorsSketchInactive },
                { SystemColor_e.SketchContour, swUserPreferenceIntegerValue_e.swShadedSketchContourColor },
                { SystemColor_e.SketchExploded, swUserPreferenceIntegerValue_e.swSketchExplodedColor }
            };

            m_UserPrefToSystemColorMap = m_SystemColorToUserPrefMap.ToDictionary(x => x.Value, x => x.Key);
        }

        internal static Color? GetColor(IComponent2 ownerComp,
            Func<swInConfigurationOpts_e, string[], double[]> getColorAction) 
        {
            GetColorScope(ownerComp, out swInConfigurationOpts_e confOpts, out string[] confs);
            
            var matPrps = getColorAction.Invoke(confOpts, confs);

            return FromMaterialProperties(matPrps);
        }

        internal static void SetColor(Color? color,
            IComponent2 ownerComp,
            Action<double[], swInConfigurationOpts_e, string[]> setColorAction, 
            Action<swInConfigurationOpts_e, string[]> removeColorAction) 
        {
            GetColorScope(ownerComp, out swInConfigurationOpts_e confOpts, out string[] confs);

            if (color.HasValue)
            {
                var matPrps = ToMaterialProperties(color.Value);

                setColorAction.Invoke(matPrps, confOpts, confs);
            }
            else 
            {
                removeColorAction?.Invoke(confOpts, confs);
            }
        }

        internal static Color? FromMaterialProperties(double[] matPrps) 
        {
            if (matPrps == null || matPrps.Length != 9)
            {
                return null;
            }

            var r = matPrps[0];
            var g = matPrps[1];
            var b = matPrps[2];
            var a = matPrps[7];

            if (r == -1 || g == -1 || b == -1) 
            {
                return null;
            }

            return Color.FromArgb(a < 0 ? 1 : (int)((1 - a) * 255), 
                (int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        internal static double[] ToMaterialProperties(Color color)
            => new double[] 
            {
                color.R / 255d,
                color.G / 255d,
                color.B / 255d, 
                1, 1, 0.5, 0.4, 
                (255 - color.A) / 255d, 
                0
            };

        internal static void GetColorScope(IComponent2 comp, out swInConfigurationOpts_e confOpts, out string[] confs) 
        {
            confOpts = comp != null 
                ? swInConfigurationOpts_e.swSpecifyConfiguration 
                : swInConfigurationOpts_e.swThisConfiguration;
            
            confs = comp != null 
                ? new string[] { comp.ReferencedConfiguration } 
                : null;
        }

        internal static swUserPreferenceIntegerValue_e ConvertSystemColor(SystemColor_e systemColor)
        {
            if (m_SystemColorToUserPrefMap.TryGetValue(systemColor, out var userPref))
            {
                return userPref;
            }
            else 
            {
                throw new NotSupportedException();
            }
        }

        internal static SystemColor_e ConvertToSystemColor(swUserPreferenceIntegerValue_e userPref)
        {
            if (m_UserPrefToSystemColorMap.TryGetValue(userPref, out var sysColor))
            {
                return sysColor;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}

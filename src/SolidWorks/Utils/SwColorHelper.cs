using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal static class SwColorHelper
    {
        internal static Color? GetColor<TEnt>(TEnt ent, IComponent2 entComp,
            Func<swInConfigurationOpts_e, string[], double[]> getColorAction) 
        {
            GetColorScope(entComp, out swInConfigurationOpts_e confOpts, out string[] confs);
            
            var matPrps = getColorAction.Invoke(confOpts, confs);

            return FromMaterialProperties(matPrps);
        }

        internal static void SetColor<TEnt>(TEnt ent, Color? color,
            IComponent2 entComp,
            Action<double[], swInConfigurationOpts_e, string[]> setColorAction, 
            Action<swInConfigurationOpts_e, string[]> removeColorAction) 
        {
            GetColorScope(entComp, out swInConfigurationOpts_e confOpts, out string[] confs);

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
    }
}

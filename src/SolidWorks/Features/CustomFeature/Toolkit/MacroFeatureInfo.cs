//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Xarial.XCad.Features.CustomFeature.Attributes;
using Xarial.XCad.Reflection;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.Features.CustomFeature.Toolkit
{
    internal static class MacroFeatureInfo
    {
        internal static string GetBaseName<TMacroFeature>()
            where TMacroFeature : SwMacroFeatureDefinition
        {
            return GetBaseName(typeof(TMacroFeature));
        }

        internal static string GetBaseName(Type macroFeatType)
        {
            if (!typeof(SwMacroFeatureDefinition).IsAssignableFrom(macroFeatType))
            {
                throw new InvalidCastException(
                    $"{macroFeatType.FullName} must inherit {typeof(SwMacroFeatureDefinition).FullName}");
            }

            string baseName = "";

            macroFeatType.TryGetAttribute<DisplayNameAttribute>(a =>
            {
                baseName = a.DisplayName;
            });

            if (string.IsNullOrEmpty(baseName))
            {
                baseName = macroFeatType.Name;
            }

            return baseName;
        }

        internal static string GetProgId<TMacroFeature>()
            where TMacroFeature : SwMacroFeatureDefinition
        {
            return GetProgId(typeof(TMacroFeature));
        }

        internal static string GetProgId(Type macroFeatType)
        {
            if (!typeof(SwMacroFeatureDefinition).IsAssignableFrom(macroFeatType))
            {
                throw new InvalidCastException(
                    $"{macroFeatType.FullName} must inherit {typeof(SwMacroFeatureDefinition).FullName}");
            }

            string progId = "";

            if (!macroFeatType.TryGetAttribute<ProgIdAttribute>(a => progId = a.Value))
            {
                progId = macroFeatType.FullName;
            }

            return progId;
        }
    }
}
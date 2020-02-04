//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Features.CustomFeature.Enums;

namespace Xarial.XCad.Features.CustomFeature.Attributes
{
    /// <summary>
    /// Provides additional options for macro feature
    /// </summary>
    public class CustomFeatureOptionsAttribute : Attribute
    {
        public CustomFeatureOptions_e Flags { get; private set; }
        public string BaseName { get; private set; }
        public string Provider { get; private set; }

        /// <summary>
        /// Options for macro feature
        /// </summary>
        /// <param name="baseName">Base name of the custom feature.
        /// This is a default name assigned to the feature when created followed by the index</param>
        /// <param name="flags">Additional options for custom feature</param>
        public CustomFeatureOptionsAttribute(string baseName,
            CustomFeatureOptions_e flags = CustomFeatureOptions_e.Default)
            : this(baseName, "", flags)
        {
        }

        /// <inheritdoc cref="OptionsAttribute(string, CustomFeatureOptions_e)"/>
        /// <param name="provider">Default message to display when custom feature cannot be loaded
        /// The provided text is displayed in the What's Wrong dialog of</param>
        public CustomFeatureOptionsAttribute(string baseName, string provider,
            CustomFeatureOptions_e flags = CustomFeatureOptions_e.Default)
        {
            Flags = flags;
            BaseName = baseName;
            Provider = provider;
        }
    }
}
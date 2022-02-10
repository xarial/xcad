//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Features.CustomFeature.Enums;

namespace Xarial.XCad.Features.CustomFeature.Attributes
{
    /// <summary>
    /// Provides additional options for custom feature
    /// </summary>
    public class CustomFeatureOptionsAttribute : Attribute
    {
        public CustomFeatureOptions_e Flags { get; }

        /// <summary>
        /// Options for macro feature
        /// </summary>
        /// <param name="baseName">Base name of the custom feature.
        /// This is a default name assigned to the feature when created followed by the index</param>
        /// <param name="flags">Additional options for custom feature</param>
        public CustomFeatureOptionsAttribute(
            CustomFeatureOptions_e flags)
        {
            Flags = flags;
        }
    }
}
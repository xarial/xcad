//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.Features.CustomFeature.Enums
{
    /// <summary>
    /// Options of the <see cref="IXCustomFeature"/>
    /// </summary>
    [Flags]
    public enum CustomFeatureOptions_e
    {
        /// <summary>
        /// Default options
        /// </summary>
        Default = 0,

        /// <summary>
        /// Custom feature should be always at the bottom of the feature tree
        /// </summary>
        AlwaysAtEnd = 1,

        /// <summary>
        /// Custom feature can be patterned
        /// </summary>
        Patternable = 2,

        /// <summary>
        /// Castom feature can be dragged
        /// </summary>
        Dragable = 4,

        /// <summary>
        /// Custom feature does not cache the body
        /// </summary>
        NoCachedBody = 8
    }
}
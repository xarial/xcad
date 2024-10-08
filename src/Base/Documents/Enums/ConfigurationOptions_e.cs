//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.Documents.Enums
{
    /// <summary>
    /// Options of <see cref="IXConfiguration"/>
    /// </summary>
    [Flags]
    public enum ConfigurationOptions_e 
    {
        /// <summary>
        /// Supress newly added components in assembly by default
        /// </summary>
        SuppressNewComponents = 1,

        /// <summary>
        /// Suppress newly added features by default
        /// </summary>
        SuppressNewFeatures = 2,

        /// <summary>
        /// Use description of the configuration in the BOM table
        /// </summary>
        UseConfigurationDescriptionInBom = 4,

        /// <summary>
        /// Use user-defined name of the configuration in the BOM
        /// </summary>
        UseUserDefinedNameInBom = 8
    }
}
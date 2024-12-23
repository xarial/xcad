//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Features.CustomFeature.Enums
{
    /// <summary>
    /// Type of the dimension associated with <see cref="IXCustomFeature"/>
    /// </summary>
    public enum CustomFeatureDimensionType_e
    {
        /// <summary>
        /// Linear
        /// </summary>
        Linear = 2,

        /// <summary>
        /// Angular
        /// </summary>
        Angular = 3,

        /// <summary>
        /// Radial
        /// </summary>
        Radial = 5
    }
}
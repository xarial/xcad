//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.Features.CustomFeature.Enums
{
    [Flags]
    public enum CustomFeatureOutdateState_e
    {
        /// <summary>
        /// All parameters are up-to-date
        /// </summary>
        UpToDate = 0,

        /// <summary>
        /// Macro feature icon has changed and cannot be updated
        /// </summary>
        Icons = 1 << 0,

        /// <summary>
        /// Macro feature dimensions have changed and cannot be upgraded
        /// </summary>
        Dimensions = 1 << 1
    }
}
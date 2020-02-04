//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Features.CustomFeature.Enums
{
    /// <summary>
    /// Reason of macro feature handler unloading. Used in <see cref="Services.ICustomFeatureHandler.Unload(CustomFeatureUnloadReason_e)"/>
    /// </summary>
    public enum CustomFeatureUnloadReason_e
    {
        /// <summary>
        /// Model containing this macro feature is closed
        /// </summary>
        ModelClosed,

        /// <summary>
        /// This macro feature is deleted
        /// </summary>
        Deleted
    }
}
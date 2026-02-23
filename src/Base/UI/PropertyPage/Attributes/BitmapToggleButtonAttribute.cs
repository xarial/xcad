//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Reflection;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Apply custom icon for the toggle button in the toggled off state
    /// </summary>
    public class BitmapToggleButtonAttribute : BitmapButtonAttribute
    {
        /// <summary>
        /// Icon for the toggle off state of the checkable bitmap button
        /// </summary>
        public IXImage ToggledOffIcon { get; }

        /// <summary>
        /// Effect applied when button is in the toggled off state
        /// </summary>
        public BitmapEffect_e ToggledOffEffect { get; }

        public BitmapToggleButtonAttribute(Type resType, string imgResName, string toggledOffImgResName) 
            : base(resType, imgResName)
        {
            ToggledOffIcon = ResourceHelper.GetResource<IXImage>(resType, toggledOffImgResName);
        }

        public BitmapToggleButtonAttribute(Type resType, string imgResName,
            BitmapEffect_e toggledOffEffect)
            : base(resType, imgResName)
        {
            ToggledOffIcon = ResourceHelper.GetResource<IXImage>(resType, imgResName);
            ToggledOffEffect = toggledOffEffect;
        }
    }
}

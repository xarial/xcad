//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.Toolkit.Exceptions
{
    /// <summary>
    /// Exception indicates that the transparency key <see cref="Base.IIcon.TransparencyKey"/> is different for
    /// some icons in the icons group passed to <see cref="Utils.IconsConverter.ConvertIconsGroup(Base.IIcon[])"/>
    /// </summary>
    public class IconTransparencyMismatchException : InvalidOperationException
    {
        public IconTransparencyMismatchException(int index)
            : base($"Transparency color of icon at index {index} doesn't match the group transparency")
        {
        }
    }
}
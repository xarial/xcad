//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Drawing;
using Xarial.XCad.Reflection;
using Xarial.XCad.UI;

namespace Xarial.XCad.Base.Attributes
{
    /// <summary>
    /// General icon for any controls or objects
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class IconAttribute : Attribute
    {
        /// <summary>
        /// Image assigned to this icon
        /// </summary>
        public virtual IXImage Icon { get; }

        /// <summary>
        /// Constructor without icon initialization
        /// </summary>
        protected IconAttribute() 
        {
        }

        /// <summary>
        /// Constructor to be used in dynamic controls
        /// </summary>
        /// <param name="icon"></param>
        public IconAttribute(IXImage icon) 
        {
            Icon = icon;
        }

        /// <param name="resType">Type of the static class (usually Resources)</param>
        /// <param name="masterResName">Resource name of the master icon</param>
        public IconAttribute(Type resType, string masterResName)
        {
            Icon = ResourceHelper.GetResource<IXImage>(resType, masterResName);
        }
    }
}
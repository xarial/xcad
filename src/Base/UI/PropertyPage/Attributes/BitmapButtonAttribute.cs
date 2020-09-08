using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Reflection;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    public interface IBitmapButtonConstructor
    {
    }

    /// <summary>
    /// Attribute indicates that current property should be rendered as bitmap button
    /// </summary>
    /// <remarks>This attribute is only applicable for <see cref="bool"/> and <see cref="Action"/> types.
    /// Checkable button will be rendered for the first case
    public class BitmapButtonAttribute : Attribute, ISpecificConstructorAttribute
    {
        public Type ConstructorType { get; }

        /// <summary>
        /// Image assigned to this icon
        /// </summary>
        public IXImage Icon { get; }

        public int Width { get; }
        public int Height { get; }

        /// <param name="resType">Type of the static class (usually Resources)</param>
        /// <param name="masterResName">Resource name of the master icon</param>        
        /// <param name="width">Button width</param>
        /// <param name="height">Button height</param>
        public BitmapButtonAttribute(Type resType, string masterResName, int width = 24, int height = 24)
        {
            ConstructorType = typeof(IBitmapButtonConstructor);
            Icon = ResourceHelper.GetResource<IXImage>(resType, masterResName);
            
            Width = width;
            Height = height;
        }
    }
}

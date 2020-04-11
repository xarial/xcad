using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    public interface ICustomControlConstructor 
    {
    }

    public class CustomControlAttribute : Attribute, ISpecificConstructorAttribute
    {
        public Type ConstructorType { get; }
        public Type ControlType { get; }

        public CustomControlAttribute(Type controlType) 
        {
            ConstructorType = typeof(ICustomControlConstructor);
            ControlType = controlType;
        }
    }
}

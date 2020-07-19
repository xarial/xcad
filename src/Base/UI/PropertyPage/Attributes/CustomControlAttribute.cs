//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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

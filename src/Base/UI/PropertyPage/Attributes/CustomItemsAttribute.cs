//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Services;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    public interface ICustomItemsComboBoxControlConstructor
    {
    }

    public class CustomItemsAttribute : Attribute, ISpecificConstructorAttribute
    {
        public Type ConstructorType { get; }

        public Type CustomItemsProviderType { get; }

        public CustomItemsAttribute(Type customItemsProviderType)
        {
            if (!typeof(ICustomItemsProvider).IsAssignableFrom(customItemsProviderType))
            {
                throw new InvalidCastException($"{customItemsProviderType.FullName} doesn't implement {typeof(ICustomItemsProvider).FullName}");
            }

            CustomItemsProviderType = customItemsProviderType;
            ConstructorType = typeof(ICustomItemsComboBoxControlConstructor);
        }
    }
}

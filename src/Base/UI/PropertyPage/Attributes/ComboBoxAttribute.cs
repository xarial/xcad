//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.UI.PropertyPage.Structures;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    public interface ICustomItemsComboBoxControlConstructor
    {
    }

    /// <summary>
    /// Indicates that the current property must be rendered as combo box
    /// </summary>
    public class ComboBoxAttribute : ItemsSourceControlAttribute, ISpecificConstructorAttribute
    {
        public Type ConstructorType => typeof(ICustomItemsComboBoxControlConstructor);
        
        /// <inheritdoc/>
        public ComboBoxAttribute(Type customItemsProviderType, params object[] dependencies) : base(customItemsProviderType, dependencies)
        {
        }

        /// <inheritdoc/>
        public ComboBoxAttribute(params object[] items) : base(items)
        {
        }
    }
}

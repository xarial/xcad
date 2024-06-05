//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
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
    public interface IListBoxControlConstructor
    {
    }

    /// <summary>
    /// Indicates that the current property must be rendered as list box
    /// </summary>
    public class ListBoxAttribute : ItemsSourceControlAttribute, ISpecificConstructorAttribute
    {
        public Type ConstructorType => typeof(IListBoxControlConstructor);

        /// <summary>
        /// Use this constructor on the <see cref="Enum"/> to render enum as list box
        /// </summary>
        public ListBoxAttribute() 
        {
        }

        /// <inheritdoc/>
        public ListBoxAttribute(Type customItemsProviderType, params object[] dependencies) : base(customItemsProviderType, dependencies)
        {
        }

        /// <inheritdoc/>
        public ListBoxAttribute(params object[] items) : base(items)
        {
        }
    }
}

//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Toolkit.Blazor.Controls;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Core;

namespace Xarial.XCad.Toolkit.Blazor.Constructors
{
    public abstract class BlazorPropertyManagerPageComboBoxConstructorBase<TVal>
        : BlazorPropertyManagerPageBaseControlConstructor<BlazorPropertyManagerPageComboBox<TVal>>
    {
        protected BlazorPropertyManagerPageComboBoxConstructorBase(IXApplication app) : base(app)
        {
        }

        protected override BlazorPropertyManagerPageComboBox<TVal> Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new BlazorPropertyManagerPageComboBox<TVal>(m_App, parentGroup, atts, metadata);
    }

    /// <summary>
    /// Default renderer for any enum-typed property
    /// </summary>
    [DefaultType(typeof(SpecialTypes.EnumType))]
    public class BlazorPropertyManagerPageEnumComboBoxConstructor
        : BlazorPropertyManagerPageComboBoxConstructorBase<Enum>
    {
        public BlazorPropertyManagerPageEnumComboBoxConstructor(IXApplication app) : base(app)
        {
        }
    }

    /// <summary>
    /// Renderer selected explicitly via <see cref="ComboBoxAttribute"/>, for any property type
    /// </summary>
    public class BlazorPropertyManagerPageCustomItemsComboBoxConstructor
        : BlazorPropertyManagerPageComboBoxConstructorBase<object>, ICustomItemsComboBoxControlConstructor
    {
        public BlazorPropertyManagerPageCustomItemsComboBoxConstructor(IXApplication app) : base(app)
        {
        }
    }
}

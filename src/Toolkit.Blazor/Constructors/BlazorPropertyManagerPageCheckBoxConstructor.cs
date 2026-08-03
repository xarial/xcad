//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Toolkit.Blazor.Controls;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.Toolkit.Blazor.Constructors
{
    [DefaultType(typeof(bool))]
    public class BlazorPropertyManagerPageCheckBoxConstructor
        : BlazorPropertyManagerPageBaseControlConstructor<BlazorPropertyManagerPageCheckBox>
    {
        public BlazorPropertyManagerPageCheckBoxConstructor(IXApplication app) : base(app)
        {
        }

        protected override BlazorPropertyManagerPageCheckBox Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new BlazorPropertyManagerPageCheckBox(parentGroup, atts, metadata);
    }
}

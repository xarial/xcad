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
    [DefaultType(typeof(int))]
    [DefaultType(typeof(short))]
    [DefaultType(typeof(double))]
    [DefaultType(typeof(decimal))]
    [DefaultType(typeof(float))]
    public class BlazorPropertyManagerPageNumberBoxConstructor
        : BlazorPropertyManagerPageBaseControlConstructor<BlazorPropertyManagerPageNumberBox>
    {
        public BlazorPropertyManagerPageNumberBoxConstructor(IXApplication app) : base(app)
        {
        }

        protected override BlazorPropertyManagerPageNumberBox Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new BlazorPropertyManagerPageNumberBox(parentGroup, atts, metadata);
    }
}

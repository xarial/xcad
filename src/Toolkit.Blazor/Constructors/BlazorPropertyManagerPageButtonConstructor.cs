//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Toolkit.Blazor.Controls;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.Toolkit.Blazor.Constructors
{
    [DefaultType(typeof(Action))]
    public class BlazorPropertyManagerPageButtonConstructor
        : BlazorPropertyManagerPageBaseControlConstructor<BlazorPropertyManagerPageButton>
    {
        public BlazorPropertyManagerPageButtonConstructor(IXApplication app) : base(app)
        {
        }

        protected override BlazorPropertyManagerPageButton Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new BlazorPropertyManagerPageButton(parentGroup, atts, metadata);
    }
}

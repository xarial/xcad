//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Toolkit.Blazor.Controls;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Constructors;

namespace Xarial.XCad.Toolkit.Blazor.Constructors
{
    /// <summary>
    /// Renderer selected explicitly via <see cref="TabAttribute"/>
    /// </summary>
    public class BlazorPropertyManagerPageTabConstructor
        : GroupConstructor<BlazorPropertyManagerPageTab, BlazorPropertyManagerPagePage>, ITabConstructor
    {
        protected override BlazorPropertyManagerPageTab Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new BlazorPropertyManagerPageTab(parentGroup, atts, metadata);
    }
}

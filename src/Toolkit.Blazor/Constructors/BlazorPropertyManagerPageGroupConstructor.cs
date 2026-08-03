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
using Xarial.XCad.Utils.PageBuilder.Constructors;
using Xarial.XCad.Utils.PageBuilder.Core;

namespace Xarial.XCad.Toolkit.Blazor.Constructors
{
    /// <summary>
    /// Default renderer for any nested complex-type property (e.g. a nested settings class)
    /// </summary>
    [DefaultType(typeof(SpecialTypes.ComplexType))]
    public class BlazorPropertyManagerPageGroupConstructor : GroupConstructor<BlazorPropertyManagerPageGroup, BlazorPropertyManagerPagePage>
    {
        protected override BlazorPropertyManagerPageGroup Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new BlazorPropertyManagerPageGroup(parentGroup, atts, metadata);
    }
}

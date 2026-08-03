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

namespace Xarial.XCad.Toolkit.Blazor.Constructors
{
    /// <summary>
    /// Renderer selected explicitly via <see cref="CustomControlAttribute"/>
    /// </summary>
    public class BlazorPropertyManagerPageCustomControlConstructor
        : BlazorPropertyManagerPageBaseControlConstructor<BlazorPropertyManagerPageCustomControl>, ICustomControlConstructor
    {
        public BlazorPropertyManagerPageCustomControlConstructor(IXApplication app) : base(app)
        {
        }

        protected override BlazorPropertyManagerPageCustomControl Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new BlazorPropertyManagerPageCustomControl(parentGroup, atts, metadata);
    }
}

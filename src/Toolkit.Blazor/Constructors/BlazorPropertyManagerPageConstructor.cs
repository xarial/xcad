//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Toolkit.Blazor.Controls;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Constructors;

namespace Xarial.XCad.Toolkit.Blazor.Constructors
{
    public class BlazorPropertyManagerPageConstructor : PageConstructor<BlazorPropertyManagerPagePage>
    {
        protected override BlazorPropertyManagerPagePage Create(IAttributeSet atts)
            => new BlazorPropertyManagerPagePage(atts);
    }
}

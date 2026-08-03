//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Toolkit.Blazor.Controls;
using Xarial.XCad.Utils.PageBuilder.Constructors;

namespace Xarial.XCad.Toolkit.Blazor.Constructors
{
    public abstract class BlazorPropertyManagerPageBaseControlConstructor<TControl>
        : ControlConstructor<TControl, BlazorPropertyManagerPageGroup, BlazorPropertyManagerPagePage>
        where TControl : IBlazorPropertyManagerPageControl
    {
        protected readonly IXApplication m_App;

        protected BlazorPropertyManagerPageBaseControlConstructor(IXApplication app)
        {
            m_App = app;
        }
    }
}

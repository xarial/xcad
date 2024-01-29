//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Utils.PageBuilder.Base
{
    public interface IPage : IGroup
    {
        IBindingManager Binding { get; }
    }
}
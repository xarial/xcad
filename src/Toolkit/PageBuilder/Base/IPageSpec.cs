//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.Utils.PageBuilder.Base
{
    public interface IPageSpec
    {
        string Title { get; }
        PageOptions_e Options { get; }
    }
}
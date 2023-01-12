//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Utils.PageBuilder.Base
{
    public interface IPageElementConstructor
    {
        IControl Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int idRange);
    }
}
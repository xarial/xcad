//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Utils.PageBuilder.Constructors
{
    public abstract class PageElementConstructor<TElem, TGroup, TPage> : IPageElementConstructor
            where TGroup : IGroup
            where TPage : IPage
            where TElem : IControl
    {
        IControl IPageElementConstructor.Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int idRange) => Create(parentGroup, atts, metadata, ref idRange);

        protected abstract TElem Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds);
    }
}
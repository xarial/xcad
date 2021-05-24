//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.Utils.PageBuilder.Constructors
{
    public abstract class PageElementConstructor<TElem, TGroup, TPage> : IPageElementConstructor<TGroup, TPage>
            where TGroup : IGroup
            where TPage : IPage
            where TElem : IControl
    {
        protected abstract TElem Create(TPage page, IAttributeSet atts, IMetadata metadata);

        protected abstract TElem Create(TGroup group, IAttributeSet atts, IMetadata metadata);

        protected virtual TElem Create(TPage page, IAttributeSet atts, IMetadata metadata, ref int idRange)
        {
            return Create(page, atts, metadata);
        }

        protected virtual TElem Create(TGroup group, IAttributeSet atts, IMetadata metadata, ref int idRange)
        {
            return Create(group, atts, metadata);
        }

        IControl IPageElementConstructor<TGroup, TPage>.Create(TPage page, IAttributeSet atts, IMetadata metadata, ref int idRange)
        {
            return Create(page, atts, metadata, ref idRange);
        }

        IControl IPageElementConstructor<TGroup, TPage>.Create(TGroup group, IAttributeSet atts, IMetadata metadata, ref int idRange)
        {
            return Create(group, atts, metadata, ref idRange);
        }
    }
}
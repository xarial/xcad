//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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
        protected abstract TElem Create(TPage page, IAttributeSet atts);

        protected abstract TElem Create(TGroup group, IAttributeSet atts);

        protected virtual TElem Create(TPage page, IAttributeSet atts, ref int idRange)
        {
            return Create(page, atts);
        }

        protected virtual TElem Create(TGroup group, IAttributeSet atts, ref int idRange)
        {
            return Create(group, atts);
        }

        IControl IPageElementConstructor<TGroup, TPage>.Create(TPage page, IAttributeSet atts, ref int idRange)
        {
            return Create(page, atts, ref idRange);
        }

        IControl IPageElementConstructor<TGroup, TPage>.Create(TGroup group, IAttributeSet atts, ref int idRange)
        {
            return Create(group, atts, ref idRange);
        }
    }
}
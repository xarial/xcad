//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Linq;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal abstract class PropertyManagerPageGroupBase : Group, IPropertyManagerPageElementEx
    {
        internal SwPropertyManagerPageHandler Handler { get; }
        internal PropertyManagerPagePage ParentPage { get; }

        protected PropertyManagerPageGroupBase(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata)
            : base(atts.Id, atts.Tag, metadata)
        {
            Handler = parentGroup.GetHandler();

            switch (parentGroup)
            {
                case PropertyManagerPagePage page:
                    ParentPage = page;
                    break;

                case PropertyManagerPageGroupBase grp:
                    ParentPage = grp.ParentPage;
                    break;

                default:
                    throw new NotSupportedException();
            }
        }
    }

    internal static class PropertyManagaerPageGroupBaseExtension 
    {
        internal static SwPropertyManagerPageHandler GetHandler(this IGroup group)
        {
            switch (group)
            {
                case PropertyManagerPagePage page:
                    return page.Handler;
                case PropertyManagerPageGroupBase grp:
                    return grp.Handler;
                default:
                    throw new NotSupportedException();
            }
        }
    }

    internal abstract class PropertyManagerPageGroupBase<TSwControlGroup> : PropertyManagerPageGroupBase
        where TSwControlGroup : class
    {
        private readonly SwApplication m_App;

        protected readonly IIconsCreator m_IconsConv;

        protected readonly TSwControlGroup m_SpecificGroup;

        protected readonly SwPropertyManagerPageHandler m_Handler;

        protected PropertyManagerPageGroupBase(SwApplication app, IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, IIconsCreator iconsConv, ref int numberOfUsedIds) 
            : base(parentGroup, atts, metadata)
        {
            m_App = app;
            m_IconsConv = iconsConv;

            m_Handler = parentGroup.GetHandler();

            InitData(atts, metadata);
            m_SpecificGroup = Create(parentGroup, atts, metadata);
            SetOptions(m_SpecificGroup, atts);
        }

        public override void ShowTooltip(string title, string msg)
        {
            m_App.Sw.HideBubbleTooltip();
            m_App.Sw.ShowBubbleTooltipAt2(0, 0, (int)swArrowPosition.swArrowLeftTop,
                title, msg, (int)swBitMaps.swBitMapNone,
                "", "", 0, (int)swLinkString.swLinkStringNone, "", "");
        }

        protected virtual void InitData(IAttributeSet atts, IMetadata[] metadata)
        {
        }

        protected abstract TSwControlGroup Create(IGroup host, IAttributeSet atts, IMetadata[] metadata);

        protected virtual void SetOptions(TSwControlGroup grp, IAttributeSet atts)
        {
        }
    }
}
//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal abstract class PropertyManagerPageGroupBase : Group, IPropertyManagerPageElementEx
    {
        public ISldWorks App { get; private set; }
        internal SwPropertyManagerPageHandler Handler { get; private set; }

        internal PropertyManagerPagePage ParentPage { get; private set; }

        public override void ShowTooltip(string title, string msg)
        {
            App.HideBubbleTooltip();
            App.ShowBubbleTooltipAt2(0, 0, (int)swArrowPosition.swArrowLeftTop,
                title, msg, (int)swBitMaps.swBitMapNone,
                "", "", 0, (int)swLinkString.swLinkStringNone, "", "");
        }
        
        internal PropertyManagerPageGroupBase(int id, object tag, SwPropertyManagerPageHandler handler,
            ISldWorks app, PropertyManagerPagePage parentPage, IMetadata[] metadata) : base(id, tag, metadata)
        {
            Handler = handler;
            App = app;
            ParentPage = parentPage;
        }
    }
}
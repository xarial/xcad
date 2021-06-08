//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal abstract class PropertyManagerPageGroupBase : Group, IPropertyManagerPageElementEx
    {
        public ISldWorks App { get; private set; }
        internal SwPropertyManagerPageHandler Handler { get; private set; }

        internal PropertyManagerPagePage ParentPage { get; private set; }

        internal PropertyManagerPageGroupBase(int id, object tag, SwPropertyManagerPageHandler handler,
            ISldWorks app, PropertyManagerPagePage parentPage) : base(id, tag)
        {
            Handler = handler;
            App = app;
            ParentPage = parentPage;
        }
    }
}
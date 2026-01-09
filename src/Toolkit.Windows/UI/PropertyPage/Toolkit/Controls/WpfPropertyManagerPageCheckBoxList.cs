//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Windows;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Templates;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls
{
    internal class WpfPropertyManagerPageCheckBoxList : WpfPropertyManagerPageItemsSourceControl<object>
    {
        public override DataTemplate Template { get; }

        protected override event ControlValueChangedDelegate<object> ValueChanged;

        public WpfPropertyManagerPageCheckBoxList(IXApplication app, IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata)
            : base(app, parentGroup, atts, metadata, iconConv)
        {
            Template = ControlTemplates.CheckBoxList;
        }

        protected override object GetSpecificValue()
        {
            return null;
        }

        protected override void SetSpecificValue(object value)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                base.Dispose(disposing);
            }
        }

        protected override ItemsControlManager<object> CreateItemsControlManager(IXApplication app, IAttributeSet atts, IMetadata[] metadata)
        {
            throw new NotImplementedException();
        }
    }
}
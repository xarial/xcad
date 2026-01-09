//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Windows;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Templates;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls
{
    internal class WpfPropertyManagerPageNumberBox : WpfPropertyManagerPageControl<object>
    {
        public override DataTemplate Template { get; }

        protected override event ControlValueChangedDelegate<object> ValueChanged;

        public WpfPropertyManagerPageNumberBox(IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata)
            : base(parentGroup, atts, metadata, iconConv)
        {
            Template = ControlTemplates.NumberBox;
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
    }
}
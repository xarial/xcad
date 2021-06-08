//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    [DefaultType(typeof(string))]
    internal class PropertyManagerPageTextBoxControlConstructor
        : PropertyManagerPageBaseControlConstructor<PropertyManagerPageTextBoxControl, IPropertyManagerPageTextbox>
    {
        public PropertyManagerPageTextBoxControlConstructor(ISldWorks app, IIconsCreator iconsConv)
            : base(app, swPropertyManagerPageControlType_e.swControlType_Textbox, iconsConv)
        {
        }

        protected override PropertyManagerPageTextBoxControl CreateControl(
            IPropertyManagerPageTextbox swCtrl, IAttributeSet atts, IMetadata metadata, 
            SwPropertyManagerPageHandler handler, short height)
        {
            if (height != -1)
            {
                swCtrl.Height = height;
            }

            if (atts.Has<TextBoxOptionsAttribute>())
            {
                var style = atts.Get<TextBoxOptionsAttribute>();

                if (style.Style != 0)
                {
                    swCtrl.Style = (int)style.Style;
                }
            }

            return new PropertyManagerPageTextBoxControl(atts.Id, atts.Tag, swCtrl, handler);
        }
    }
}
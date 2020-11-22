//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    [DefaultType(typeof(int))]
    [DefaultType(typeof(double))]
    internal class PropertyManagerPageNumberBoxConstructor
        : PropertyManagerPageBaseControlConstructor<PropertyManagerPageNumberBoxControl, IPropertyManagerPageNumberbox>
    {
        public PropertyManagerPageNumberBoxConstructor(ISldWorks app, IIconsCreator iconsConv)
            : base(app, swPropertyManagerPageControlType_e.swControlType_Numberbox, iconsConv)
        {
        }

        protected override PropertyManagerPageNumberBoxControl CreateControl(
            IPropertyManagerPageNumberbox swCtrl, IAttributeSet atts, SwPropertyManagerPageHandler handler, short height)
        {
            if (height != -1)
            {
                swCtrl.Height = height;
            }

            if (atts.Has<NumberBoxOptionsAttribute>())
            {
                var style = atts.Get<NumberBoxOptionsAttribute>();

                if (style.Style != 0)
                {
                    swCtrl.Style = (int)style.Style;
                }

                if (style.Units != 0)
                {
                    swCtrl.SetRange2((int)style.Units, style.Minimum, style.Maximum,
                        style.Inclusive, style.Increment, style.FastIncrement, style.SlowIncrement);
                }
            }

            return new PropertyManagerPageNumberBoxControl(atts.Id, atts.Tag, swCtrl, handler);
        }
    }
}
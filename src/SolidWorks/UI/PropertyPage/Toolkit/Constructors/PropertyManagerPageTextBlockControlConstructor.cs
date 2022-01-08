//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Drawing;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    internal class PropertyManagerPageTextBlockControlConstructor
        : PropertyManagerPageBaseControlConstructor<PropertyManagerPageTextBlockControl, IPropertyManagerPageLabel>, ITextBlockConstructor
    {
        public PropertyManagerPageTextBlockControlConstructor(ISldWorks app, IIconsCreator iconsConv)
            : base(app, swPropertyManagerPageControlType_e.swControlType_Label, iconsConv)
        {
        }

        protected override PropertyManagerPageTextBlockControl CreateControl(
            IPropertyManagerPageLabel swCtrl, IAttributeSet atts, IMetadata[] metadata, 
            SwPropertyManagerPageHandler handler, short height, IPropertyManagerPageLabel label)
        {
            if (height != -1)
            {
                swCtrl.Height = height;
            }

            var fontStyle = FontStyle_e.Default;
            var font = "";
            var textColor = default(KnownColor?);

            if (atts.Has<ControlOptionsAttribute>())
            {
                textColor = atts.Get<ControlOptionsAttribute>().TextColor;
            }

            if (atts.Has<TextBlockOptionsAttribute>())
            {
                var style = atts.Get<TextBlockOptionsAttribute>();

                swCtrl.Style = (int)style.TextAlignment;

                fontStyle = style.FontStyle;
                font = style.Font;

                swCtrl.SetLabelOptions(fontStyle, font, textColor);
            }

            return new PropertyManagerPageTextBlockControl(atts.Id, atts.Tag, swCtrl, fontStyle, font, textColor, handler, label, metadata);
        }
    }
}
//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    [DefaultType(typeof(int))]
    [DefaultType(typeof(double))]
    [DefaultType(typeof(decimal))]
    [DefaultType(typeof(float))]
    internal class PropertyManagerPageNumberBoxConstructor
        : PropertyManagerPageBaseControlConstructor<PropertyManagerPageNumberBoxControl, IPropertyManagerPageNumberbox>
    {
        public PropertyManagerPageNumberBoxConstructor(SwApplication app, IIconsCreator iconsConv)
            : base(app, iconsConv)
        {
        }

        protected override PropertyManagerPageNumberBoxControl Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new PropertyManagerPageNumberBoxControl(m_App, parentGroup, m_IconConv, atts, metadata, ref numberOfUsedIds);
    }
}
//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Drawing;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    internal class PropertyManagerPageTextBlockControlConstructor
        : PropertyManagerPageBaseControlConstructor<PropertyManagerPageTextBlockControl, IPropertyManagerPageLabel>, ITextBlockConstructor
    {
        public PropertyManagerPageTextBlockControlConstructor(SwApplication app, IIconsCreator iconsConv)
            : base(app, iconsConv)
        {
        }

        protected override PropertyManagerPageTextBlockControl Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new PropertyManagerPageTextBlockControl(m_App, parentGroup, m_IconConv, atts, metadata, ref numberOfUsedIds);
    }
}
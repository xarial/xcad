//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Constructors;
using Xarial.XCad.Utils.PageBuilder.PageElements;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    internal class PropertyManagerPageTabConstructor
        : GroupConstructor<PropertyManagerPageTabControl, PropertyManagerPagePage>,
        IPropertyManagerPageElementConstructor, ITabConstructor
    {
        public Type ControlType => typeof(PropertyManagerPageTabControl);

        private readonly SwApplication m_App;
        private readonly IIconsCreator m_IconsConv;

        public PropertyManagerPageTabConstructor(SwApplication app, IIconsCreator iconsConv)
        {
            m_App = app;
            m_IconsConv = iconsConv;
        }

        public void PostProcessControls(IEnumerable<IPropertyManagerPageControlEx> ctrls)
        {
            //TODO: not used
        }

        protected override PropertyManagerPageTabControl Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new PropertyManagerPageTabControl(m_App, parentGroup, atts, metadata, m_IconsConv, ref numberOfUsedIds);
    }
}
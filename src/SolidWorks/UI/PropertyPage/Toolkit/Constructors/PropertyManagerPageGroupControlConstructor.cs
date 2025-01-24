//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Constructors;
using Xarial.XCad.Utils.PageBuilder.Core;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    [DefaultType(typeof(SpecialTypes.ComplexType))]
    internal class PropertyManagerPageGroupControlConstructor
        : GroupConstructor<PropertyManagerPageGroupControl, PropertyManagerPagePage>,
        IPropertyManagerPageElementConstructor
    {
        public Type ControlType => typeof(PropertyManagerPageGroupControl);

        private readonly SwApplication m_App;
        private readonly IIconsCreator m_IconsConv;

        internal PropertyManagerPageGroupControlConstructor(SwApplication app, IIconsCreator iconsConv)
        {
            m_App = app;
            m_IconsConv = iconsConv;
        }

        public void PostProcessControls(IEnumerable<IPropertyManagerPageControlEx> ctrls)
        {
            //TODO: not used
        }

        protected override PropertyManagerPageGroupControl Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new PropertyManagerPageGroupControl(m_App, parentGroup, atts, metadata, m_IconsConv, ref numberOfUsedIds);
    }
}
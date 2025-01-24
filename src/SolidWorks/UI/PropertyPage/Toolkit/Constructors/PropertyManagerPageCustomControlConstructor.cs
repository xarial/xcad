//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.Commands.Exceptions;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.UI.Toolkit;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    internal class PropertyManagerPageCustomControlConstructor
        : PropertyManagerPageBaseControlConstructor<PropertyManagerPageCustomControl, IPropertyManagerPageWindowFromHandle>, ICustomControlConstructor
    {
        public PropertyManagerPageCustomControlConstructor(SwApplication app, IIconsCreator iconsConv)
            : base(app, iconsConv)
        {
        }

        protected override PropertyManagerPageCustomControl Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new PropertyManagerPageCustomControl(m_App, parentGroup, m_IconConv, atts, metadata, ref numberOfUsedIds);
    }
}
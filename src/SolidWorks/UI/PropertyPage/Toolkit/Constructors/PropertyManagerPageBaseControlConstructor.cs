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
using System.Diagnostics;
using System.Drawing;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Utils;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Constructors;
using Xarial.XCad.Utils.PageBuilder.PageElements;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    internal interface IPropertyManagerPageElementConstructor : IPageElementConstructor
    {
        Type ControlType { get; }
        void PostProcessControls(IEnumerable<IPropertyManagerPageControlEx> ctrls);
    }

    internal abstract class PropertyManagerPageBaseControlConstructor<TControl, TControlSw>
            : ControlConstructor<TControl, PropertyManagerPageGroupBase, PropertyManagerPagePage>,
            IPropertyManagerPageElementConstructor
            where TControl : IPropertyManagerPageControlEx
            where TControlSw : class
    {
        public Type ControlType => typeof(TControl);
        protected readonly IIconsCreator m_IconConv;
        protected readonly SwApplication m_App;

        protected PropertyManagerPageBaseControlConstructor(SwApplication app, IIconsCreator iconsConv)
        {
            m_App = app;
            m_IconConv = iconsConv;
        }

        public virtual void PostProcessControls(IEnumerable<IPropertyManagerPageControlEx> ctrls)
        {
        }
    }
}
//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.Utils.PageBuilder.Constructors;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Constructors
{
    internal abstract class WpfPropertyManagerPageBaseControlConstructor<TControl>
            : ControlConstructor<TControl, WpfPropertyManagerPageGroup, WpfPropertyManagerPagePage>
            where TControl : IWpfPropertyManagerPageControl
    {
        protected readonly IXApplication m_App;

        protected readonly IIconsCreator m_IconConv;

        protected WpfPropertyManagerPageBaseControlConstructor(IXApplication app, IIconsCreator iconsConv)
        {
            m_App = app;

            m_IconConv = iconsConv;
        }

        public virtual void PostProcessControls(IEnumerable<IWpfPropertyManagerPageControl> ctrls)
        {
        }
    }
}
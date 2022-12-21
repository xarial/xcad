//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using System.Drawing;
using Xarial.XCad.SolidWorks.Base;
using Xarial.XCad.Toolkit.Base;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.UI.Commands.Toolkit.Structures
{
    internal class TaskPaneIcon : IIcon
    {
        protected readonly IXImage m_Icon;

        public virtual Color TransparencyKey => Color.White;

        public bool IsPermanent => false;

        internal TaskPaneIcon(IXImage icon)
        {
            m_Icon = icon;
            IconSizes = new IIconSpec[]
            {
                new IconSpec(m_Icon, new Size(16, 18))
            };
        }

        public virtual IIconSpec[] IconSizes { get; }
    }
}
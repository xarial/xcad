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
    internal class CommandGroupHighResIcon : CommandGroupIcon
    {
        internal CommandGroupHighResIcon(IXImage icon) : base(icon)
        {
            IconSizes = new IIconSpec[]
            {
                new IconSpec(m_Icon, new Size(20, 20)),
                new IconSpec(m_Icon, new Size(32, 32)),
                new IconSpec(m_Icon, new Size(40, 40)),
                new IconSpec(m_Icon, new Size(64, 64)),
                new IconSpec(m_Icon, new Size(96, 96)),
                new IconSpec(m_Icon, new Size(128, 128))
            };
        }

        public override IIconSpec[] IconSizes { get; }
    }
}
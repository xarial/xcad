//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using System.Drawing;
using Xarial.XCad.SolidWorks.Base;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.UI.Commands.Toolkit.Structures
{
    internal class TaskPaneHighResIcon : TaskPaneIcon
    {
        internal TaskPaneHighResIcon(IXImage icon) : base(icon)
        {
        }

        public override IEnumerable<IIconSpec> GetIconSizes()
        {
            yield return new IconSpec(m_Icon, new Size(20, 20));
            yield return new IconSpec(m_Icon, new Size(32, 32));
            yield return new IconSpec(m_Icon, new Size(40, 40));
            yield return new IconSpec(m_Icon, new Size(64, 64));
            yield return new IconSpec(m_Icon, new Size(96, 96));
            yield return new IconSpec(m_Icon, new Size(128, 128));
        }
    }
}
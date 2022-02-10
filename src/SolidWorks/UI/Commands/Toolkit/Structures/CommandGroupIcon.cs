//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using System.Drawing;
using Xarial.XCad.SolidWorks.Base;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.UI.Commands.Toolkit.Structures
{
    internal class CommandGroupIcon : IIcon
    {
        protected readonly IXImage m_Icon;

        private static readonly Color m_CommandTransparencyKey
                    = Color.FromArgb(192, 192, 192);

        public virtual Color TransparencyKey
        {
            get
            {
                return m_CommandTransparencyKey;
            }
        }

        internal CommandGroupIcon(IXImage icon)
        {
            m_Icon = icon;
        }

        public virtual IEnumerable<IIconSpec> GetIconSizes()
        {
            yield return new IconSpec(m_Icon, new Size(16, 16));
            yield return new IconSpec(m_Icon, new Size(24, 24));
        }
    }
}
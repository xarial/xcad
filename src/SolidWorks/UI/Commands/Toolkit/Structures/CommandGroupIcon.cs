//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using System.Drawing;
using Xarial.XCad.SolidWorks.Base;

namespace Xarial.XCad.SolidWorks.UI.Commands.Toolkit.Structures
{
    internal class CommandGroupIcon : IIcon
    {
        protected readonly Image m_Icon;

        private static readonly Color m_CommandTransparencyKey
                    = Color.FromArgb(192, 192, 192);

        public virtual Color TransparencyKey
        {
            get
            {
                return m_CommandTransparencyKey;
            }
        }

        internal CommandGroupIcon(Image icon)
        {
            m_Icon = icon;
        }

        public virtual IEnumerable<IconSizeInfo> GetIconSizes()
        {
            yield return new IconSizeInfo(m_Icon, new Size(16, 16));
            yield return new IconSizeInfo(m_Icon, new Size(24, 24));
        }
    }
}
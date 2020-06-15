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
    internal class TaskPaneIcon : IIcon
    {
        protected readonly Image m_Icon;

        private static readonly Color m_TransparencyKey
                    = Color.White;

        public virtual Color TransparencyKey
        {
            get
            {
                return m_TransparencyKey;
            }
        }

        internal TaskPaneIcon(Image icon)
        {
            m_Icon = icon;
        }

        public virtual IEnumerable<IconSizeInfo> GetIconSizes()
        {
            yield return new IconSizeInfo(m_Icon, new Size(16, 18));
        }
    }
}
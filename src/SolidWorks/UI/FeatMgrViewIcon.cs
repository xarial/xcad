﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using System.Drawing;
using Xarial.XCad.SolidWorks.Base;
using Xarial.XCad.Toolkit.Base;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.UI
{
    internal class FeatMgrViewIcon : IIcon
    {
        protected readonly IXImage m_Icon;

        private static readonly Color m_TransparencyKey
                    = Color.White;

        public virtual Color TransparencyKey => m_TransparencyKey;

        public bool IsPermanent => false;

        public IconImageFormat_e Format => IconImageFormat_e.Bmp;

        internal FeatMgrViewIcon(IXImage icon)
        {
            m_Icon = icon;

            IconSizes = new IIconSpec[]
            {
                new IconSpec(m_Icon, new Size(18, 18))
            };
        }

        public virtual IIconSpec[] IconSizes { get; }
    }
}

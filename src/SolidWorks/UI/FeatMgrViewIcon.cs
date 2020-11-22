//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using System.Drawing;
using Xarial.XCad.SolidWorks.Base;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.UI
{
    internal class FeatMgrViewIcon : IIcon
    {
        protected readonly IXImage m_Icon;

        private static readonly Color m_TransparencyKey
                    = Color.White;

        public virtual Color TransparencyKey
        {
            get
            {
                return m_TransparencyKey;
            }
        }

        internal FeatMgrViewIcon(IXImage icon)
        {
            m_Icon = icon;
        }

        public virtual IEnumerable<IIconSpec> GetIconSizes()
        {
            yield return new IconSpec(m_Icon, new Size(18, 18));
        }
    }
}

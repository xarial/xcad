using System.Collections.Generic;
using System.Drawing;
using Xarial.XCad.SolidWorks.Base;

namespace Xarial.XCad.SolidWorks.UI
{
    internal class FeatMgrViewIcon : IIcon
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

        internal FeatMgrViewIcon(Image icon)
        {
            m_Icon = icon;
        }

        public virtual IEnumerable<IconSizeInfo> GetIconSizes()
        {
            yield return new IconSizeInfo(m_Icon, new Size(18, 18));
        }
    }
}

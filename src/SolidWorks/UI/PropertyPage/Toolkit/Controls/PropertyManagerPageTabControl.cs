//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System.ComponentModel;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal class PropertyManagerPageTabControl : PropertyManagerPageGroupBase
    {
        public IPropertyManagerPageTab Tab { get; private set; }

        /// <summary>
        /// Not supported
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Enabled
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

        /// <summary>
        /// Not supported
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Visible
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

        internal PropertyManagerPageTabControl(int id, object tag, SwPropertyManagerPageHandler handler,
            IPropertyManagerPageTab tab,
            ISldWorks app, PropertyManagerPagePage parentPage) : base(id, tag, handler, app, parentPage)
        {
            Tab = tab;
        }
    }
}
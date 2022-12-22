//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    public interface IPropertyManagerPageTabEx
    {
        /// <summary>
        /// Pointer to the underlying tab box
        /// </summary>
        IPropertyManagerPageTab Tab { get; }
    }

    internal class PropertyManagerPageTabControl : PropertyManagerPageGroupBase<IPropertyManagerPageTab>, IPropertyManagerPageTabEx
    {
        public IPropertyManagerPageTab Tab => m_SpecificGroup;

        /// <summary>
        /// Not supported
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Enabled
        {
            get => true;
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
            get => true;
            set
            {
            }
        }

        private IImageCollection m_TabIcon;

        public PropertyManagerPageTabControl(SwApplication app, IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, IIconsCreator iconsConv, ref int numberOfUsedIds) 
            : base(app, parentGroup, atts, metadata, iconsConv, ref numberOfUsedIds)
        {
        }

        protected override IPropertyManagerPageTab Create(IGroup host, IAttributeSet atts, IMetadata[] metadata)
        {
            PropertyManagerPagePage hostPage;

            switch (host) 
            {
                case PropertyManagerPagePage page:
                    hostPage = page;
                    break;

                case PropertyManagerPageGroupBase grp:
                    //NOTE: nested tabs are not supported in SOLIDWORKS, creating the group in page instead
                    hostPage = grp.ParentPage;
                    break;

                default:
                    throw new NotSupportedException();
            }

            const int OPTIONS_NOT_USED = 0;

            var icon = atts.ControlDescriptor?.Icon;

            if (icon == null)
            {
                icon = atts.ContextType?.TryGetAttribute<IconAttribute>()?.Icon;
            }

            string iconPath = "";

            if (icon != null)
            {
                m_TabIcon = m_IconsConv.ConvertIcon(new TabIcon(icon));

                //NOTE: tab icon must be in 256 color bitmap, otherwise it is not displayed
                TryConvertIconTo8bit(m_TabIcon.FilePaths[0]);
            }

            return hostPage.Page.AddTab(atts.Id, atts.Name, iconPath, OPTIONS_NOT_USED);
        }

        private void TryConvertIconTo8bit(string path)
        {
            try
            {
                using (var img = Image.FromFile(path))
                {
                    using (var srcBmp = new Bitmap(img))
                    {
                        using (var destBmp = srcBmp.Clone(new Rectangle(new Point(0, 0), srcBmp.Size), PixelFormat.Format8bppIndexed))
                        {
                            img.Dispose();
                            destBmp.Save(path, ImageFormat.Bmp);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                base.Dispose(disposing);
                m_TabIcon?.Dispose();
            }
        }
    }
}
//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Constructors;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    internal class PropertyManagerPageTabConstructor
        : GroupConstructor<PropertyManagerPageGroupBase, PropertyManagerPagePage>,
        IPropertyManagerPageElementConstructor, ITabConstructor
    {
        public Type ControlType
        {
            get
            {
                return typeof(PropertyManagerPageGroupBase);
            }
        }

        private readonly IIconsCreator m_IconsConv;

        public PropertyManagerPageTabConstructor(IIconsCreator iconsConv)
        {
            m_IconsConv = iconsConv;
        }

        public void PostProcessControls(IEnumerable<IPropertyManagerPageControlEx> ctrls)
        {
            //TODO: not used
        }

        protected override PropertyManagerPageGroupBase Create(PropertyManagerPageGroupBase group, IAttributeSet atts, IMetadata metadata)
        {
            //NOTE: nested tabs are not supported in SOLIDWORKS, creating the group in page instead
            return Create(group.ParentPage, atts, metadata);
        }

        protected override PropertyManagerPageGroupBase Create(PropertyManagerPagePage page, IAttributeSet atts, IMetadata metadata)
        {
            const int OPTIONS_NOT_USED = 0;

            var icon = atts.ControlDescriptor?.Icon;

            if (icon == null)
            {
                icon = atts.ContextType?.TryGetAttribute<IconAttribute>()?.Icon;
            }

            string iconPath = "";

            if (icon != null)
            {
                iconPath = m_IconsConv.ConvertIcon(new TabIcon(icon)).First();

                //NOTE: tab icon must be in 256 color bitmap, otherwise it is not displayed
                TryConvertIconTo8bit(iconPath);
            }

            var tab = page.Page.AddTab(atts.Id, atts.Name,
                iconPath, OPTIONS_NOT_USED) as IPropertyManagerPageTab;

            return new PropertyManagerPageTabControl(atts.Id, atts.Tag,
                page.Handler, tab, page.App, page);
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
    }
}
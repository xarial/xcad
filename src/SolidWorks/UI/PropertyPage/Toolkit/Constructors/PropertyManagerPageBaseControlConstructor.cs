//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Constructors;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    internal interface IPropertyManagerPageElementConstructor
        : IPageElementConstructor<PropertyManagerPageGroupBase, PropertyManagerPagePage>
    {
        Type ControlType { get; }

        void PostProcessControls(IEnumerable<IPropertyManagerPageControlEx> ctrls);
    }

    internal abstract class PropertyManagerPageBaseControlConstructor<TControl, TControlSw>
            : ControlConstructor<TControl, PropertyManagerPageGroupBase, PropertyManagerPagePage>,
            IPropertyManagerPageElementConstructor
            where TControl : IPropertyManagerPageControlEx
            where TControlSw : class
    {
        public Type ControlType
        {
            get
            {
                return typeof(TControl);
            }
        }

        private swPropertyManagerPageControlType_e m_Type;
        private IconsConverter m_IconConv;

        protected readonly ISldWorks m_App;

        protected PropertyManagerPageBaseControlConstructor(ISldWorks app, swPropertyManagerPageControlType_e type,
            IconsConverter iconsConv)
        {
            m_App = app;
            m_IconConv = iconsConv;
            m_Type = type;
        }

        protected override TControl Create(PropertyManagerPageGroupBase group, IAttributeSet atts)
        {
            var opts = GetControlOptions(atts);

            TControlSw swCtrl = null;

            if (group is PropertyManagerPageGroupControl)
            {
                swCtrl = CreateSwControlInGroup((group as PropertyManagerPageGroupControl).Group, opts, atts) as TControlSw;
            }
            else if (group is PropertyManagerPageTabControl)
            {
                swCtrl = CreateSwControlInTab((group as PropertyManagerPageTabControl).Tab, opts, atts) as TControlSw;
            }
            else
            {
                throw new NotSupportedException("Type of group is not supported");
            }

            AssignControlAttributes(swCtrl, opts, atts);

            return CreateControl(swCtrl, atts, group.Handler, opts.Height);
        }

        protected override TControl Create(PropertyManagerPagePage page, IAttributeSet atts)
        {
            var opts = GetControlOptions(atts);

            var swCtrl = CreateSwControlInPage(page.Page, opts, atts) as TControlSw;

            AssignControlAttributes(swCtrl, opts, atts);

            return CreateControl(swCtrl, atts, page.Handler, opts.Height);
        }

        protected virtual TControlSw CreateSwControlInPage(IPropertyManagerPage2 page,
            ControlOptionsAttribute opts, IAttributeSet atts)
        {
            if (m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2014, 1))
            {
                return page.AddControl2(atts.Id, (short)m_Type, atts.Name,
                    (short)opts.Align, (short)opts.Options, atts.Description) as TControlSw;
            }
            else
            {
                return page.AddControl(atts.Id, (short)m_Type, atts.Name,
                    (short)opts.Align, (short)opts.Options, atts.Description) as TControlSw;
            }
        }

        protected virtual TControlSw CreateSwControlInGroup(IPropertyManagerPageGroup group,
            ControlOptionsAttribute opts, IAttributeSet atts)
        {
            if (m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2014, 1))
            {
                return group.AddControl2(atts.Id, (short)m_Type, atts.Name,
                    (short)opts.Align, (short)opts.Options, atts.Description) as TControlSw;
            }
            else
            {
                return group.AddControl(atts.Id, (short)m_Type, atts.Name,
                    (short)opts.Align, (short)opts.Options, atts.Description) as TControlSw;
            }
        }

        protected virtual TControlSw CreateSwControlInTab(IPropertyManagerPageTab tab,
            ControlOptionsAttribute opts, IAttributeSet atts)
        {
            if (m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2014, 1))
            {
                return tab.AddControl2(atts.Id, (short)m_Type, atts.Name,
                    (short)opts.Align, (short)opts.Options, atts.Description) as TControlSw;
            }
            else
            {
                return tab.AddControl(atts.Id, (short)m_Type, atts.Name,
                    (short)opts.Align, (short)opts.Options, atts.Description) as TControlSw;
            }
        }

        protected abstract TControl CreateControl(TControlSw swCtrl, IAttributeSet atts, SwPropertyManagerPageHandler handler, short height);

        private ControlOptionsAttribute GetControlOptions(IAttributeSet atts)
        {
            ControlOptionsAttribute opts;

            if (atts.Has<ControlOptionsAttribute>())
            {
                opts = atts.Get<ControlOptionsAttribute>();
            }
            else
            {
                opts = new ControlOptionsAttribute();
            }

            return opts;
        }

        private void AssignControlAttributes(TControlSw ctrl, ControlOptionsAttribute opts, IAttributeSet atts)
        {
            var swCtrl = ctrl as IPropertyManagerPageControl;

            if (opts.BackgroundColor != 0)
            {
                swCtrl.BackgroundColor = ConvertColor(opts.BackgroundColor);
            }

            if (opts.TextColor != 0)
            {
                swCtrl.TextColor = ConvertColor(opts.TextColor);
            }

            if (opts.Left != -1)
            {
                swCtrl.Left = opts.Left;
            }

            if (opts.Top != -1)
            {
                swCtrl.Top = opts.Top;
            }

            if (opts.Width != -1)
            {
                swCtrl.Width = opts.Width;
            }

            if (opts.ResizeOptions != 0)
            {
                swCtrl.OptionsForResize = (int)opts.ResizeOptions;
            }

            ControlIcon icon = null;

            var commonIcon = atts.BoundMemberInfo?.TryGetAttribute<IconAttribute>()?.Icon;

            if (commonIcon != null)
            {
                icon = new ControlIcon(commonIcon);
            }

            if (atts.Has<ControlAttributionAttribute>())
            {
                var attribution = atts.Get<ControlAttributionAttribute>();

                if (attribution.StandardIcon != 0)
                {
                    swCtrl.SetStandardPictureLabel((int)attribution.StandardIcon);
                }
                //else if (attribution.Icon != null)
                //{
                //    icon = attribution.Icon;
                //}
            }

            if (icon != null)
            {
                var icons = m_IconConv.ConvertIcon(icon);
                var res = swCtrl.SetPictureLabelByName(icons[0], icons[1]);
                Debug.Assert(res);
            }
        }

        protected int ConvertColor(KnownColor knownColor)
        {
            var color = Color.FromKnownColor(knownColor);

            return (color.R << 0) | (color.G << 8) | (color.B << 16);
        }

        public virtual void PostProcessControls(IEnumerable<IPropertyManagerPageControlEx> ctrls)
        {
        }
    }
}
//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Utils;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
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

        protected swPropertyManagerPageControlType_e m_Type;
        private IIconsCreator m_IconConv;

        protected readonly ISldWorks m_App;

        protected PropertyManagerPageBaseControlConstructor(ISldWorks app, swPropertyManagerPageControlType_e type,
            IIconsCreator iconsConv)
        {
            m_App = app;
            m_IconConv = iconsConv;
            m_Type = type;
        }

        protected override TControl Create(PropertyManagerPageGroupBase group, IAttributeSet atts, IMetadata metadata, ref int numberOfUsedIds)
        {
            var opts = GetControlOptions(atts);

            TControlSw swCtrl;
            IPropertyManagerPageLabel label;

            if (group is PropertyManagerPageGroupControl)
            {
                var grp = (group as PropertyManagerPageGroupControl).Group;
                AddLabelIfNeeded(grp, atts, ref numberOfUsedIds, out label);
                swCtrl = CreateSwControl(grp, opts, atts) as TControlSw;
            }
            else if (group is PropertyManagerPageTabControl)
            {
                var tab = (group as PropertyManagerPageTabControl).Tab;
                AddLabelIfNeeded(tab, atts, ref numberOfUsedIds, out label);
                swCtrl = CreateSwControl(tab, opts, atts) as TControlSw;
            }
            else
            {
                throw new NotSupportedException("Type of group is not supported");
            }

            AssignControlAttributes(swCtrl, opts, atts);
            
            return CreateControl(swCtrl, atts, metadata, group.Handler, opts.Height, label);
        }

        protected override TControl Create(PropertyManagerPagePage page, IAttributeSet atts, IMetadata metadata, ref int numberOfUsedIds)
        {
            var opts = GetControlOptions(atts);

            AddLabelIfNeeded(page.Page, atts, ref numberOfUsedIds, out IPropertyManagerPageLabel label);

            var swCtrl = CreateSwControl(page.Page, opts, atts) as TControlSw;

            AssignControlAttributes(swCtrl, opts, atts);

            return CreateControl(swCtrl, atts, metadata, page.Handler, opts.Height, label);
        }
        
        protected virtual TControlSw CreateSwControl(object host, ControlOptionsAttribute opts, IAttributeSet atts)
        {
            var legacy = !m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2014, 1);

            switch (host)
            {
                case IPropertyManagerPage2 page:
                    if (!legacy)
                    {
                        return page.AddControl2(atts.Id, (short)m_Type, atts.Name,
                            (short)opts.Align, (short)opts.Options, atts.Description) as TControlSw;
                    }
                    else
                    {
                        return page.AddControl(atts.Id, (short)m_Type, atts.Name,
                            (short)opts.Align, (short)opts.Options, atts.Description) as TControlSw;
                    }

                case IPropertyManagerPageTab tab:
                    if (!legacy)
                    {
                        return tab.AddControl2(atts.Id, (short)m_Type, atts.Name,
                            (short)opts.Align, (short)opts.Options, atts.Description) as TControlSw;
                    }
                    else
                    {
                        return tab.AddControl(atts.Id, (short)m_Type, atts.Name,
                            (short)opts.Align, (short)opts.Options, atts.Description) as TControlSw;
                    }

                case IPropertyManagerPageGroup group:
                    if (!legacy)
                    {
                        return group.AddControl2(atts.Id, (short)m_Type, atts.Name,
                            (short)opts.Align, (short)opts.Options, atts.Description) as TControlSw;
                    }
                    else
                    {
                        return group.AddControl(atts.Id, (short)m_Type, atts.Name,
                            (short)opts.Align, (short)opts.Options, atts.Description) as TControlSw;
                    }

                default:
                    throw new NotSupportedException("Host is not supported");
            }
        }

        private bool AddLabelIfNeeded(object host, IAttributeSet atts, ref int numberOfUsedIds, out IPropertyManagerPageLabel label) 
        {
            if (atts.Has<LabelAttribute>())
            {
                numberOfUsedIds++;

                var id = atts.Id + numberOfUsedIds - 1;

                var labelAtt = atts.Get<LabelAttribute>();

                var legacy = !m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2014, 1);

                var type = swPropertyManagerPageControlType_e.swControlType_Label;
                var align = (swPropertyManagerPageControlLeftAlign_e)labelAtt.Align;
                var opts = swAddControlOptions_e.swControlOptions_Enabled | swAddControlOptions_e.swControlOptions_SmallGapAbove | swAddControlOptions_e.swControlOptions_Visible;

                switch (host)
                {
                    case IPropertyManagerPage2 page:
                        if (!legacy)
                        {
                            label =(IPropertyManagerPageLabel)page.AddControl2(id, (short)type, labelAtt.Caption,
                                (short)align, (short)opts, atts.Description);
                        }
                        else
                        {
                            label = (IPropertyManagerPageLabel)page.AddControl(id, (short)type, labelAtt.Caption,
                                (short)align, (short)opts, atts.Description);
                        }
                        break;

                    case IPropertyManagerPageTab tab:
                        if (!legacy)
                        {
                            label = (IPropertyManagerPageLabel)tab.AddControl2(id, (short)type, labelAtt.Caption,
                                (short)align, (short)opts, atts.Description);
                        }
                        else
                        {
                            label = (IPropertyManagerPageLabel)tab.AddControl(id, (short)type, labelAtt.Caption,
                                (short)align, (short)opts, atts.Description);
                        }
                        break;

                    case IPropertyManagerPageGroup group:
                        if (!legacy)
                        {
                            label = (IPropertyManagerPageLabel)group.AddControl2(id, (short)type, labelAtt.Caption,
                                (short)align, (short)opts, atts.Description);
                        }
                        else
                        {
                            label = (IPropertyManagerPageLabel)group.AddControl(id, (short)type, labelAtt.Caption,
                                (short)align, (short)opts, atts.Description);
                        }
                        break;

                    default:
                        throw new NotSupportedException("Host is not supported");
                }

                label.Caption = labelAtt.Caption;

                switch (labelAtt.FontStyle)
                {
                    case LabelFontStyle_e.Bold:
                        label.Bold[0, (short)(labelAtt.Caption.Length - 1)] = true;
                        break;

                    case LabelFontStyle_e.Italic:
                        label.Italic[0, (short)(labelAtt.Caption.Length - 1)] = true;
                        break;

                    case LabelFontStyle_e.Underline:
                        label.Underline[0, (short)(labelAtt.Caption.Length - 1)] = (int)swPropMgrPageLabelUnderlineStyle_e.swPropMgrPageLabel_SolidUnderline;
                        break;
                }

                return true;
            }
            else 
            {
                label = null;
                return false;
            }
        }
        
        protected abstract TControl CreateControl(TControlSw swCtrl, IAttributeSet atts,
            IMetadata metadata, SwPropertyManagerPageHandler handler, short height, IPropertyManagerPageLabel label);
        
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

            var commonIcon = atts.ControlDescriptor?.Icon;

            if (commonIcon != null)
            {
                icon = new ControlIcon(commonIcon);
            }

            var hasIcon = false;

            if (atts.Has<StandardControlIconAttribute>())
            {
                var attribution = atts.Get<StandardControlIconAttribute>();

                if (attribution.Label != 0)
                {
                    hasIcon = true;
                    swCtrl.SetStandardPictureLabel((int)attribution.Label);
                }
            }

            if (icon != null)
            {
                hasIcon = true;
                var icons = m_IconConv.ConvertIcon(icon);
                var res = swCtrl.SetPictureLabelByName(icons[0], icons[1]);
                Debug.Assert(res);
            }

            if (!hasIcon) 
            {
                var defIcon = GetDefaultBitmapLabel(atts);

                if (defIcon.HasValue) 
                {
                    swCtrl.SetStandardPictureLabel((int)defIcon.Value);
                }
            }
        }

        protected int ConvertColor(KnownColor knownColor)
        {
            var color = Color.FromKnownColor(knownColor);

            return ColorUtils.ToColorRef(color);
        }

        public virtual void PostProcessControls(IEnumerable<IPropertyManagerPageControlEx> ctrls)
        {
        }
    }
}
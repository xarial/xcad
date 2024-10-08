//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Diagnostics;
using System.Drawing;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Icons;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Utils;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    /// <summary>
    /// Base wrapper around native SOLIDWORKS Property Manager Page controls (i.e. TextBox, SelectionBox, NumberBox etc.)
    /// </summary>
    public interface IPropertyManagerPageControlEx : IControl, IPropertyManagerPageElementEx
    {
        /// <summary>
        /// Pointer to the native SOLIDWORKS control of type <see href="http://help.solidworks.com/2012/english/api/sldworksapi/solidworks.interop.sldworks~solidworks.interop.sldworks.ipropertymanagerpagecontrol.html"/>
        /// </summary>
        IPropertyManagerPageControl SwControl { get; }
    }

    internal abstract class PropertyManagerPageBaseControl<TVal, TSwControl>
        : Control<TVal>, IPropertyManagerPageControlEx
        where TSwControl : class
    {
        private readonly IPropertyManagerPageLabel m_Label;
        protected readonly IIconsCreator m_IconConv;
        protected readonly SwPropertyManagerPageHandler m_Handler;
        protected readonly SwApplication m_App;

        private IImageCollection m_CustomIcon;

        public override Type ValueType { get; }

        protected PropertyManagerPageBaseControl(SwApplication app, IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata, swPropertyManagerPageControlType_e type, ref int numberOfUsedIds)
            : base(atts.Id, atts.Tag, metadata)
        {
            m_App = app;
            m_Handler = parentGroup.GetHandler();
            m_IconConv = iconConv;
            ValueType = atts.ContextType;

            var opts = GetControlOptions(atts);

            InitData(opts, atts);
            CreateLabelIfNeeded(parentGroup, atts, ref numberOfUsedIds, out m_Label);
            SwSpecificControl = Create(parentGroup, atts.Id, atts.Name, opts.Align, opts.Options, atts.Description, type);
            AssignControlAttributes(SwSpecificControl, opts, atts);
        }

        protected virtual void InitData(IControlOptionsAttribute opts, IAttributeSet atts) 
        {
        }

        protected virtual TSwControl Create(IGroup host, int id, string name, ControlLeftAlign_e align, AddControlOptions_e options,
            string description, swPropertyManagerPageControlType_e type) 
            => CreateSwControl<TSwControl>(host, id, name, align, options, description, type);

        protected virtual void SetOptions(TSwControl ctrl, IControlOptionsAttribute opts, IAttributeSet atts) 
        {
        }

        protected TSwControl SwSpecificControl { get; private set; }

        public override bool Enabled
        {
            get
            {
                return SwControl.Enabled;
            }
            set
            {
                SwControl.Enabled = value;

                if (m_Label != null)
                {
                    ((IPropertyManagerPageControl)m_Label).Enabled = value;
                }
            }
        }

        public override bool Visible
        {
            get
            {
                return SwControl.Visible;
            }
            set
            {
                SwControl.Visible = value;

                if (m_Label != null) 
                {
                    ((IPropertyManagerPageControl)m_Label).Visible = value;
                }
            }
        }

        public override void ShowTooltip(string title, string msg)
            => SwControl.ShowBubbleTooltip(title, msg, "");

        public IPropertyManagerPageControl SwControl
        {
            get
            {
                if (SwSpecificControl is IPropertyManagerPageControl)
                {
                    return SwSpecificControl as IPropertyManagerPageControl;
                }
                else
                {
                    throw new InvalidCastException(
                        $"Failed to cast {typeof(TSwControl).FullName} to {typeof(IPropertyManagerPageControl).FullName}");
                }
            }
        }

        public override void Focus()
        {
            //TODO: implement focusing via IPropertyManagerPage2::SetFocus
        }

        protected virtual BitmapLabelType_e? GetDefaultBitmapLabel(IAttributeSet atts) => null;

        protected virtual void AssignControlAttributes(TSwControl ctrl, IControlOptionsAttribute opts, IAttributeSet atts)
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
                m_CustomIcon = m_IconConv.ConvertIcon(icon);
                var res = swCtrl.SetPictureLabelByName(m_CustomIcon.FilePaths[0], m_CustomIcon.FilePaths[1]);
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

            SetOptions(ctrl, opts, atts);
        }

        protected int ConvertColor(KnownColor knownColor)
        {
            var color = Color.FromKnownColor(knownColor);

            return ColorUtils.ToColorRef(color);
        }

        private bool CreateLabelIfNeeded(IGroup host, IAttributeSet atts, ref int numberOfUsedIds, out IPropertyManagerPageLabel label)
        {
            if (atts.Has<LabelAttribute>())
            {
                numberOfUsedIds++;

                var id = atts.Id + numberOfUsedIds - 1;

                var labelAtt = atts.Get<LabelAttribute>();

                label = CreateSwControl<IPropertyManagerPageLabel>(host, id, "", labelAtt.Align,
                    AddControlOptions_e.Enabled | AddControlOptions_e.SmallGapAbove | AddControlOptions_e.Visible,
                    "", swPropertyManagerPageControlType_e.swControlType_Label);

                label.Caption = labelAtt.Caption;

                label.SetLabelOptions(labelAtt.FontStyle, "", null);

                return true;
            }
            else
            {
                label = null;
                return false;
            }
        }

        protected TSpecificSwControl CreateSwControl<TSpecificSwControl>(IGroup host, int id, string name, ControlLeftAlign_e align, AddControlOptions_e options,
            string description, swPropertyManagerPageControlType_e type)
            where TSpecificSwControl : class
        {
            var legacy = !m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2014, 1);

            switch (host)
            {
                case PropertyManagerPagePage page:
                    var pageCtrl = page.Page;
                    if (!legacy)
                    {
                        return (TSpecificSwControl)pageCtrl.AddControl2(id, (short)type, name,
                            (short)align, (short)options, description);
                    }
                    else
                    {
                        return (TSpecificSwControl)pageCtrl.AddControl(id, (short)type, name,
                            (short)align, (short)options, description);
                    }

                case PropertyManagerPageTabControl tab:
                    var tabCtrl = tab.Tab;
                    if (!legacy)
                    {
                        return (TSpecificSwControl)tabCtrl.AddControl2(id, (short)type, name,
                            (short)align, (short)options, description);
                    }
                    else
                    {
                        return (TSpecificSwControl)tabCtrl.AddControl(id, (short)type, name,
                            (short)align, (short)options, description);
                    }

                case PropertyManagerPageGroupControl group:
                    var grpCtrl = group.Group;
                    if (!legacy)
                    {
                        return (TSpecificSwControl)grpCtrl.AddControl2(id, (short)type, name,
                            (short)align, (short)options, description);
                    }
                    else
                    {
                        return (TSpecificSwControl)grpCtrl.AddControl(id, (short)type, name,
                            (short)align, (short)options, description);
                    }

                default:
                    throw new NotSupportedException("Host is not supported");
            }
        }

        private IControlOptionsAttribute GetControlOptions(IAttributeSet atts)
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                base.Dispose(disposing);

                m_CustomIcon?.Dispose();
            }
        }
    }
}
//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    /// <summary>
    /// Wrapper class around the group of <see href="http://help.solidworks.com/2016/english/api/sldworksapi/solidworks.interop.sldworks~solidworks.interop.sldworks.ipropertymanagerpageoption.html">IPropertyManagerPageOption </see> controls
    /// </summary>
    /// <remarks>All set properties will be applied to all controls in the group, while get will return the value of first control</remarks>
    public class PropertyManagerPageOptionBox : IPropertyManagerPageControl, IPropertyManagerPageOption
    {
        public PropertyManagerPageOptionBox(IPropertyManagerPageOption[] ctrls)
        {
            if (ctrls == null || !ctrls.Any())
            {
                throw new NullReferenceException("No controls");
            }

            Controls = ctrls;
        }

        /// <summary>
        /// Array of controls in the current option group
        /// </summary>
        public IPropertyManagerPageOption[] Controls { get; private set; }

        public int BackgroundColor
        {
            get
            {
                return (Controls.First() as IPropertyManagerPageControl).BackgroundColor;
            }
            set
            {
                ForEach<IPropertyManagerPageControl>(c => c.BackgroundColor = value);
            }
        }

        public bool Enabled
        {
            get
            {
                return (Controls.First() as IPropertyManagerPageControl).Enabled;
            }
            set
            {
                ForEach<IPropertyManagerPageControl>(c => c.Enabled = value);
            }
        }

        public short Left
        {
            get
            {
                return (Controls.First() as IPropertyManagerPageControl).Left;
            }
            set
            {
                ForEach<IPropertyManagerPageControl>(c => c.Left = value);
            }
        }

        public int OptionsForResize
        {
            get
            {
                return (Controls.First() as IPropertyManagerPageControl).OptionsForResize;
            }
            set
            {
                ForEach<IPropertyManagerPageControl>(c => c.OptionsForResize = value);
            }
        }

        public int TextColor
        {
            get
            {
                return (Controls.First() as IPropertyManagerPageControl).TextColor;
            }
            set
            {
                ForEach<IPropertyManagerPageControl>(c => c.TextColor = value);
            }
        }

        public string Tip
        {
            get
            {
                return (Controls.First() as IPropertyManagerPageControl).Tip;
            }
            set
            {
                ForEach<IPropertyManagerPageControl>(c => c.Tip = value);
            }
        }

        public short Top
        {
            get
            {
                return (Controls.First() as IPropertyManagerPageControl).Top;
            }
            set
            {
                ForEach<IPropertyManagerPageControl>(c => c.Top = value);
            }
        }

        public bool Visible
        {
            get
            {
                return (Controls.First() as IPropertyManagerPageControl).Visible;
            }
            set
            {
                ForEach<IPropertyManagerPageControl>(c => c.Visible = value);
            }
        }

        public short Width
        {
            get
            {
                return (Controls.First() as IPropertyManagerPageControl).Width;
            }
            set
            {
                ForEach<IPropertyManagerPageControl>(c => c.Width = value);
            }
        }

        public bool Checked
        {
            get
            {
                return Controls.First().Checked;
            }
            set
            {
                ForEach<IPropertyManagerPageOption>(c => c.Checked = value);
            }
        }

        public string Caption
        {
            get
            {
                return Controls.First().Caption;
            }
            set
            {
                ForEach<IPropertyManagerPageOption>(c => c.Caption = value);
            }
        }

        public int Style
        {
            get
            {
                return Controls.First().Style;
            }
            set
            {
                ForEach<IPropertyManagerPageOption>(c => c.Style = value);
            }
        }

        public PropertyManagerPageGroup GetGroupBox()
        {
            return (Controls.First() as IPropertyManagerPageControl).GetGroupBox();
        }

        public bool SetPictureLabelByName(string ColorBitmap, string MaskBitmap)
        {
            var result = true;

            ForEach<IPropertyManagerPageControl>(c => result &= c.SetPictureLabelByName(ColorBitmap, MaskBitmap));

            return result;
        }

        public bool SetStandardPictureLabel(int Bitmap)
        {
            var result = true;

            ForEach<IPropertyManagerPageControl>(c => result &= c.SetStandardPictureLabel(Bitmap));

            return result;
        }

        public void ShowBubbleTooltip(string Title, string Message, string BmpFile)
        {
            ForEach<IPropertyManagerPageControl>(c => c.ShowBubbleTooltip(Title, Message, BmpFile));
        }

        private void ForEach<TType>(Action<TType> action)
        {
            foreach (TType ctrl in Controls)
            {
                action.Invoke(ctrl);
            }
        }
    }

    internal class PropertyManagerPageOptionBoxControl : PropertyManagerPageBaseControl<Enum, PropertyManagerPageOptionBox>
    {
        private delegate IPropertyManagerPageOption ControlCreatorDelegate(int id, short controlType, string caption, short leftAlign, int options, string tip);

        protected override event ControlValueChangedDelegate<Enum> ValueChanged;

        private Enum[] m_Values;
        private string[] m_ItemNames;

        public PropertyManagerPageOptionBoxControl(SwApplication app, IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            : base(app, parentGroup, iconConv, atts, metadata, swPropertyManagerPageControlType_e.swControlType_Option, ref numberOfUsedIds)
        {
            m_Handler.OptionChecked += OnOptionChecked;
            numberOfUsedIds = m_Values.Length;
        }

        protected override void InitData(IControlOptionsAttribute opts, IAttributeSet atts)
        {
            var items = EnumExtension.GetEnumFields(atts.ContextType);
            m_Values = items.Keys.ToArray();
            m_ItemNames = items.Values.ToArray();
        }

        protected override PropertyManagerPageOptionBox Create(IGroup host, int id, string name, ControlLeftAlign_e align,
            AddControlOptions_e options, string description, swPropertyManagerPageControlType_e type)
        {
            var ctrls = new IPropertyManagerPageOption[m_ItemNames.Length];

            for (int i = 0; i < m_ItemNames.Length; i++)
            {
                var itemName = m_ItemNames[i];

                ctrls[i] = base.CreateSwControl<IPropertyManagerPageOption>(host, id + i, itemName, align, options, description, type);
            }

            return new PropertyManagerPageOptionBox(ctrls);
        }

        protected override void SetOptions(PropertyManagerPageOptionBox ctrl, IControlOptionsAttribute opts, IAttributeSet atts)
        {
            if (atts.Has<OptionBoxOptionsAttribute>())
            {
                var style = atts.Get<OptionBoxOptionsAttribute>();

                if (style.Style != 0)
                {
                    ctrl.Style = (int)style.Style;
                }
            }
        }

        private int GetIndex(int id) => id - Id;

        private void OnOptionChecked(int id)
        {
            if (id >= Id && id < (Id + m_Values.Length))
            {
                ValueChanged?.Invoke(this, m_Values[GetIndex(id)]);
            }
        }

        protected override Enum GetSpecificValue()
        {
            for (int i = 0; i < SwSpecificControl.Controls.Length; i++)
            {
                if (SwSpecificControl.Controls[i].Checked)
                {
                    return m_Values[i];
                }
            }

            //TODO: check how this condition works
            return null;
        }

        protected override void SetSpecificValue(Enum value)
        {
            var index = Array.IndexOf(m_Values, value);

            for (int i = 0; i < SwSpecificControl.Controls.Length; i++) 
            {
                SwSpecificControl.Controls[i].Checked = i == index;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Handler.OptionChecked -= OnOptionChecked;
            }
        }
    }
}
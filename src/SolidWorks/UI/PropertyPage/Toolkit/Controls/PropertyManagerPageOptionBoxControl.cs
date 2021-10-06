//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xarial.XCad.Utils.PageBuilder.PageElements;

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
        protected override event ControlValueChangedDelegate<Enum> ValueChanged;

        private ReadOnlyCollection<Enum> m_Values;

        public PropertyManagerPageOptionBoxControl(int id, object tag,
            PropertyManagerPageOptionBox optionBox, ReadOnlyCollection<Enum> values,
            SwPropertyManagerPageHandler handler, IPropertyManagerPageLabel label) : base(optionBox, id, tag, handler, label)
        {
            m_Values = values;
            m_Handler.OptionChecked += OnOptionChecked;
        }

        private int GetIndex(int id)
        {
            return id - Id;
        }

        private void OnOptionChecked(int id)
        {
            if (id >= Id && id < (Id + m_Values.Count))
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
            var index = m_Values.IndexOf(value);

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
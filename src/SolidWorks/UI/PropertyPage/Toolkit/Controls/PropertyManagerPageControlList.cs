//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Linq;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    public abstract class PropertyManagerPageControlList<T> : IPropertyManagerPageControl
    {
        /// <summary>
        /// Array of controls in the current controls list
        /// </summary>
        public virtual T[] Controls { get; }

        protected PropertyManagerPageControlList()
        {
        }

        protected PropertyManagerPageControlList(T[] ctrls)
        {
            if (ctrls?.Any() != true)
            {
                throw new NullReferenceException("No controls");
            }

            Controls = ctrls;
        }

        public int BackgroundColor
        {
            get => FirstControl().BackgroundColor;
            set => ForEachControl(c => c.BackgroundColor = value);
        }

        public bool Enabled
        {
            get => FirstControl().Enabled;
            set=> ForEachControl(c => c.Enabled = value);
        }

        public short Left
        {
            get => FirstControl().Left;
            set => ForEachControl(c => c.Left = value);
        }

        public int OptionsForResize
        {
            get => FirstControl().OptionsForResize;
            set => ForEachControl(c => c.OptionsForResize = value);
        }

        public int TextColor
        {
            get => FirstControl().TextColor;
            set => ForEachControl(c => c.TextColor = value);
        }

        public string Tip
        {
            get => FirstControl().Tip;
            set => ForEachControl(c => c.Tip = value);
        }

        public short Top
        {
            get => FirstControl().Top;
            set => ForEachControl(c => c.Top = value);
        }

        public bool Visible
        {
            get => FirstControl().Visible;
            set => ForEachControl(c => c.Visible = value);
        }

        public short Width
        {
            get => FirstControl().Width;
            set => ForEachControl(c => c.Width = value);
        }

        public PropertyManagerPageGroup GetGroupBox()
            => FirstControl().GetGroupBox();

        public bool SetPictureLabelByName(string ColorBitmap, string MaskBitmap)
        {
            var result = true;

            ForEachControl(c => result &= c.SetPictureLabelByName(ColorBitmap, MaskBitmap));

            return result;
        }

        public bool SetStandardPictureLabel(int Bitmap)
        {
            var result = true;

            ForEachControl(c => result &= c.SetStandardPictureLabel(Bitmap));

            return result;
        }

        public void ShowBubbleTooltip(string Title, string Message, string BmpFile)
            => FirstControl().ShowBubbleTooltip(Title, Message, BmpFile);

        private void ForEachControl(Action<IPropertyManagerPageControl> action)
        {
            foreach (var ctrl in Controls)
            {
                action.Invoke((IPropertyManagerPageControl)ctrl);
            }
        }

        private IPropertyManagerPageControl FirstControl() => (IPropertyManagerPageControl)Controls.First();

        protected void ForEach(Action<T> action)
        {
            foreach (var ctrl in Controls)
            {
                action.Invoke(ctrl);
            }
        }
    }
}
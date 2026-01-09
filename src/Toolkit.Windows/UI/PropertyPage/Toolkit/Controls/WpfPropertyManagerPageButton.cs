//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Windows;
using System.Windows.Input;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Templates;
using Xarial.XCad.Toolkit.Windows.UI.Wpf;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls
{
    internal class WpfPropertyManagerPageButton : WpfPropertyManagerPageControl<Action>
    {
        protected override event ControlValueChangedDelegate<Action> ValueChanged;

        private Action m_ButtonClickHandler;

        public ICommand ClickCommand { get; }

        public override DataTemplate Template { get; }

        public WpfPropertyManagerPageButton(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, IIconsCreator iconConv)
            : base(parentGroup, atts, metadata, iconConv)
        {
            Template = ControlTemplates.Button;

            ClickCommand = new RelayCommand(OnClick);
        }

        private void OnClick()
        {
            m_ButtonClickHandler.Invoke();
        }

        protected override Action GetSpecificValue() => m_ButtonClickHandler;

        protected override void SetSpecificValue(Action value) => m_ButtonClickHandler = value;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }
}
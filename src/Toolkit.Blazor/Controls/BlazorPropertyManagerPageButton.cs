//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Blazor.Controls
{
    public class BlazorPropertyManagerPageButton : BlazorPropertyManagerPageControl<Action>
    {
        protected override event ControlValueChangedDelegate<Action> ValueChanged;

        private Action m_ClickHandler;

        public BlazorPropertyManagerPageButton(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata)
            : base(parentGroup, atts, metadata)
        {
        }

        internal void Click() => m_ClickHandler?.Invoke();

        protected override Action GetSpecificValue() => m_ClickHandler;

        protected override void SetSpecificValue(Action value) => m_ClickHandler = value;
    }
}

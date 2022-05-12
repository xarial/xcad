//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.Utils.PageBuilder.Core
{
    public abstract class Binding<TDataModel> : IBinding
    {
        public event Action<IBinding> Changed;
        public event Action<IBinding> ControlUpdated;
        public event Action<IBinding> ModelUpdated;
        
        public IControl Control { get; }

        public abstract IMetadata[] Metadata { get; }

        public bool Silent { get; }

        public Binding(IControl control, bool silent)
        {
            Control = control;
            Control.ValueChanged += OnControlValueChanged;
            Silent = silent;
        }

        public void UpdateControl()
        {
            Control.Update();
            SetUserControlValue();
            ControlUpdated?.Invoke(this);
        }

        public void UpdateDataModel()
        {
            SetDataModelValue(Control.GetValue());
            ModelUpdated?.Invoke(this);
        }

        protected void RaiseChangedEvent() 
            => Changed?.Invoke(this);

        protected abstract void SetDataModelValue(object value);

        protected abstract void SetUserControlValue();

        private void OnControlValueChanged(IControl sender, object newValue)
        {
            if (!(sender is IGroup))
            {
                SetDataModelValue(newValue);
            }

            ModelUpdated?.Invoke(this);
            RaiseChangedEvent();
        }
    }
}
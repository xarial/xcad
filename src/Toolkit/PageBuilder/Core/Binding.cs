//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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
        
        public IControl Control { get; private set; }

        object IBinding.Model
        {
            get
            {
                return DataModel;
            }
            set 
            {
                DataModel = (TDataModel)value;
            }
        }

        protected virtual TDataModel DataModel { get; set; }

        public abstract IMetadata Metadata { get; }

        public Binding(IControl control)
        {
            Control = control;
            Control.ValueChanged += OnControlValueChanged;
        }

        public void UpdateControl()
        {
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
            SetDataModelValue(newValue);
            ModelUpdated?.Invoke(this);
            RaiseChangedEvent();
        }
    }
}
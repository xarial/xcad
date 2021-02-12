//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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
            SetDataModelValue();
            ModelUpdated?.Invoke(this);
        }

        protected void RaiseChangedEvent() 
            => Changed?.Invoke(this);

        protected abstract void SetDataModelValue();

        protected abstract void SetUserControlValue();

        private void OnControlValueChanged(IControl sender, object newValue)
        {
            UpdateDataModel();
            RaiseChangedEvent();
        }
    }
}
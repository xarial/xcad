//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.Utils.PageBuilder.Core
{
    /// <summary>
    /// Generic binding of the specific data model
    /// </summary>
    /// <typeparam name="TDataModel">Data model type</typeparam>
    public abstract class Binding<TDataModel> : IBinding
    {
        /// <inheritdoc/>
        public event Action<IBinding> Changed;

        /// <inheritdoc/>
        public event Action<IBinding> ControlUpdated;

        /// <inheritdoc/>
        public event Action<IBinding> ModelUpdated;

        /// <inheritdoc/>
        public IControl Control { get; }

        /// <inheritdoc/>
        public abstract IMetadata[] Metadata { get; }

        /// <inheritdoc/>
        public bool Silent { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="control">Control of this binding</param>
        /// <param name="silent">Is binding silent</param>
        public Binding(IControl control, bool silent)
        {
            Control = control;
            Control.ValueChanged += OnControlValueChanged;
            Silent = silent;
        }

        /// <inheritdoc/>
        public void UpdateControl()
        {
            Control.Update();
            SetUserControlValue();
            ControlUpdated?.Invoke(this);
        }

        /// <inheritdoc/>
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
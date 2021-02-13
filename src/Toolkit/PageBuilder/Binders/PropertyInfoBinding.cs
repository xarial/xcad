//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.UI;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Core;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.Utils.PageBuilder.Binders
{
    public class PropertyInfoBinding<TDataModel> : Binding<TDataModel>
    {
        private readonly IList<IControlDescriptor> m_Parents;

        public IControlDescriptor ControlDescriptor { get; }

        internal PropertyInfoBinding(IControl control,
            IControlDescriptor prpInfo, IList<IControlDescriptor> parents)
            : base(control)
        {
            ControlDescriptor = prpInfo;
            m_Parents = parents;
        }

        protected override TDataModel DataModel 
        {
            get => base.DataModel; 
            set
            { 
                var curModel = GetCurrentModel();

                if (curModel is INotifyPropertyChanged) 
                {
                    (curModel as INotifyPropertyChanged).PropertyChanged -= OnPropertyChanged;
                }

                base.DataModel = value;

                curModel = GetCurrentModel();

                if (curModel is INotifyPropertyChanged)
                {
                    (curModel as INotifyPropertyChanged).PropertyChanged += OnPropertyChanged;
                }
            }
        }

        protected override void SetDataModelValue()
        {
            var value = Control.GetValue();
            var curModel = GetCurrentModel();

            var curVal = ControlDescriptor.GetValue(curModel);
            var destVal = value.Cast(ControlDescriptor.DataType);

            if (!object.Equals(curVal, destVal))
            {
                ControlDescriptor.SetValue(curModel, destVal);
            }
        }

        protected override void SetUserControlValue()
        {
            var curModel = GetCurrentModel();
            var val = ControlDescriptor.GetValue(curModel);

            var curVal = Control.GetValue();

            if (!object.Equals(val, curVal))
            {
                Control.SetValue(val);
            }
        }

        private object GetCurrentModel()
        {
            object curModel = DataModel;

            if (m_Parents != null)
            {
                foreach (var parent in m_Parents)
                {
                    if (curModel == null) 
                    {
                        return null;
                    }

                    var nextModel = parent.GetValue(curModel);

                    if (nextModel == null)
                    {
                        nextModel = Activator.CreateInstance(parent.DataType);
                        parent.SetValue(curModel, nextModel);
                    }

                    curModel = nextModel;
                }
            }

            return curModel;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ControlDescriptor.Name)
            {
                SetUserControlValue();
                RaiseChangedEvent();
            }
        }
    }
}
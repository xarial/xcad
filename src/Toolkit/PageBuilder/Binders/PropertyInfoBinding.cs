//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
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
using Xarial.XCad.Toolkit.PageBuilder.Binders;
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

        private readonly IMetadata[] m_Metadata;

        private object m_CurrentDataModelContext;

        private readonly IContextProvider m_ContextProvider;

        internal PropertyInfoBinding(IControl control,
            IControlDescriptor ctrlDesc, IList<IControlDescriptor> parents, IMetadata[] metadata, IContextProvider contextProvider, bool silent)
            : base(control, silent)
        {
            ControlDescriptor = ctrlDesc;
            m_Parents = parents;
            m_Metadata = metadata;
            m_ContextProvider = contextProvider;
            m_ContextProvider.ContextChanged += OnContextChanged;
        }

        private void OnContextChanged(IContextProvider sender, object model)
        {
            if (m_CurrentDataModelContext is INotifyPropertyChanged)
            {
                (m_CurrentDataModelContext as INotifyPropertyChanged).PropertyChanged -= OnPropertyChanged;
            }
            
            m_CurrentDataModelContext = GetContextModel(model);

            if (m_CurrentDataModelContext is INotifyPropertyChanged)
            {
                (m_CurrentDataModelContext as INotifyPropertyChanged).PropertyChanged += OnPropertyChanged;
            }
        }

        public override IMetadata[] Metadata => m_Metadata;

        protected override void SetDataModelValue(object value)
        {
            var curVal = ControlDescriptor.GetValue(m_CurrentDataModelContext);
            var destVal = value.Cast(ControlDescriptor.DataType);

            if (!object.Equals(curVal, destVal))
            {
                ControlDescriptor.SetValue(m_CurrentDataModelContext, destVal);
            }
        }

        protected override void SetUserControlValue()
        {
            var val = ControlDescriptor.GetValue(m_CurrentDataModelContext);

            var curVal = Control.GetValue();

            if (!object.Equals(val, curVal))
            {
                Control.SetValue(val);
            }
        }

        private object GetContextModel(object model)
        {
            object curModel = model;

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
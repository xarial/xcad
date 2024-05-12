﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder;

namespace Xarial.XCad.Toolkit.PageBuilder.Binders
{
    public class PropertyInfoMetadata : IMetadata
    {
        public event Action<IMetadata, object> Changed;

        private readonly PropertyInfo m_PrpInfo;
        private readonly PropertyInfo[] m_Parents;

        private object m_CurrentContext;

        public object Tag { get; }

        public object Value 
        {
            get => GetValue();
            set
            {
                if (m_CurrentContext != null || m_IsStaticPrp.Value)
                {
                    m_PrpInfo.SetValue(m_CurrentContext, value);
                }
            }
        }

        private readonly IContextProvider m_ContextProvider;

        private readonly Lazy<bool> m_IsStaticPrp;

        public PropertyInfoMetadata(PropertyInfo prpInfo, PropertyInfo[] parents, object tag, IContextProvider contextProvider)
        {
            m_PrpInfo = prpInfo;
            m_IsStaticPrp = new Lazy<bool>(() => prpInfo.GetAccessors().Any(x => x.IsStatic));
            m_Parents = parents;
            Tag = tag;
            m_ContextProvider = contextProvider;
            m_ContextProvider.ContextChanged += OnContextChanged;
        }

        private void OnContextChanged(IContextProvider sender, object model)
        {
            SetDataContext(model);
        }

        private void SetDataContext(object model) 
        {
            if (m_CurrentContext is INotifyPropertyChanged)
            {
                (m_CurrentContext as INotifyPropertyChanged).PropertyChanged -= OnPropertyChanged;
            }

            m_CurrentContext = GetCurrentContext(model);

            if (m_CurrentContext is INotifyPropertyChanged)
            {
                (m_CurrentContext as INotifyPropertyChanged).PropertyChanged += OnPropertyChanged;
            }
            
            //TODO: handle INotifyCollection
        }

        private object GetCurrentContext(object curModel)
        {
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
                        nextModel = Activator.CreateInstance(parent.PropertyType);
                        parent.SetValue(curModel, nextModel);
                    }

                    curModel = nextModel;
                }
            }

            return curModel;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == m_PrpInfo.Name) 
            {
                Changed?.Invoke(this, GetValue());
            }
        }

        private object GetValue()
        {
            if (m_CurrentContext != null || m_IsStaticPrp.Value)
            {
                return m_PrpInfo.GetValue(m_CurrentContext, null);
            }
            else 
            {
                if (m_PrpInfo.PropertyType.IsValueType)
                {
                    return Activator.CreateInstance(m_PrpInfo.PropertyType);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}

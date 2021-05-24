//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.Toolkit.PageBuilder.Binders
{
    public class PropertyInfoMetadata : IMetadata
    {
        public event Action<IMetadata, object> Changed;

        private readonly PropertyInfo m_PrpInfo;
        private readonly PropertyInfo[] m_Parents;

        private object m_CurrentModel;

        public object Value 
        {
            get => GetValue();
            set => m_PrpInfo.SetValue(m_CurrentModel, value);
        }

        public PropertyInfoMetadata(PropertyInfo prpInfo, PropertyInfo[] parents)
        {
            m_PrpInfo = prpInfo;
            m_Parents = parents;
        }

        internal void SetDataModel(object model) 
        {
            if (m_CurrentModel is INotifyPropertyChanged)
            {
                (m_CurrentModel as INotifyPropertyChanged).PropertyChanged -= OnPropertyChanged;
            }

            m_CurrentModel = GetCurrentModel(model);

            if (m_CurrentModel is INotifyPropertyChanged)
            {
                (m_CurrentModel as INotifyPropertyChanged).PropertyChanged += OnPropertyChanged;
            }
            
            //TODO: handle INotifyCollection

            Changed?.Invoke(this, GetValue());
        }

        private object GetCurrentModel(object curModel)
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
            => m_PrpInfo.GetValue(m_CurrentModel, null);
    }
}

//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xarial.XCad.UI;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.Toolkit.PageBuilder.Binders
{
    /// <summary>
    /// Wrapper is used to transform context of the dynamic control to be equal to property value resolved in runtime
    /// </summary>
    public class ControlDescriptorWrapper : IControlDescriptor
    {
        public string DisplayName => m_BaseCtrlDesc.DisplayName;
        public string Description => m_BaseCtrlDesc.Description;
        public string Name => m_BaseCtrlDesc.Name;
        public IXImage Icon => m_BaseCtrlDesc.Icon;
        public Type DataType => m_BaseCtrlDesc.DataType;
        public IAttribute[] Attributes => m_BaseCtrlDesc.Attributes;

        public object GetValue(object context)
            => m_BaseCtrlDesc.GetValue(GetContext(context));

        public void SetValue(object context, object value)
            => m_BaseCtrlDesc.SetValue(GetContext(context), value);

        private readonly IControlDescriptor m_BaseCtrlDesc;
        private readonly PropertyInfo m_PrpInfo;

        public ControlDescriptorWrapper(IControlDescriptor baseCtrlDesc, PropertyInfo prpInfo) 
        {
            m_BaseCtrlDesc = baseCtrlDesc;
            m_PrpInfo = prpInfo;
        }

        private object GetContext(object context)
        {
            if (context != null)
            {
                return m_PrpInfo.GetValue(context, null);
            }
            else
            {
                return null;
            }
        }
    }
}

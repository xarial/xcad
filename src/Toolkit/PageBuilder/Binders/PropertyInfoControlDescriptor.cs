//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.UI;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.Utils.PageBuilder.Binders
{
    public class PropertyInfoControlDescriptor : IControlDescriptor
    {
        public string Name => m_PrpInfo.Name;
        public IAttribute[] Attributes { get; }

        public string DisplayName { get; }
        public string Description { get; }

        public Type DataType => m_PrpInfo.PropertyType;

        public IXImage Icon { get; }

        public object GetValue(object context)
            => m_PrpInfo.GetValue(context, null);

        public void SetValue(object context, object value)
            => m_PrpInfo.SetValue(context, value, null);

        private readonly PropertyInfo m_PrpInfo;

        public PropertyInfoControlDescriptor(PropertyInfo prpInfo) 
        {
            m_PrpInfo = prpInfo;

            var customAtts = m_PrpInfo.GetCustomAttributes(true) ?? new object[0];

            DisplayName = customAtts.OfType<DisplayNameAttribute>().FirstOrDefault()?.DisplayName;

            if (string.IsNullOrEmpty(DisplayName)) 
            {
                DisplayName = m_PrpInfo.PropertyType.GetCustomAttribute<DisplayNameAttribute>(true)?.DisplayName;
            }

            Description = customAtts.OfType<DescriptionAttribute>().FirstOrDefault()?.Description;

            if (string.IsNullOrEmpty(Description))
            {
                Description = m_PrpInfo.PropertyType.GetCustomAttribute<DescriptionAttribute>(true)?.Description;
            }

            Icon = customAtts.OfType<IconAttribute>().FirstOrDefault()?.Icon;
            Attributes = customAtts?.OfType<IAttribute>().ToArray();
        }
    }
}
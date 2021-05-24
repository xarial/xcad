//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Data;
using Xarial.XCad.SwDocumentManager.Documents;
using System.Linq;
using Xarial.XCad.SwDocumentManager.Features;

namespace Xarial.XCad.SwDocumentManager.Data
{
    internal class SwDmCutListCustomPropertiesCollection : SwDmCustomPropertiesCollection
    {
        public override int Count => (m_CutList.CutListItem.GetCustomPropertyNames() as string[])?.Length ?? 0;

        private readonly ISwDmCutListItem m_CutList;

        internal SwDmCutListCustomPropertiesCollection(ISwDmCutListItem cutList)
        {
            m_CutList = cutList;
        }

        public override IEnumerator<IXProperty> GetEnumerator()
        {
            var prpNames = m_CutList.CutListItem.GetCustomPropertyNames() as string[] ?? new string[0];
            prpNames = prpNames.Except(new string[] { SwDmConfiguration.QTY_PROPERTY }).ToArray();
            return prpNames.Select(p => CreatePropertyInstance(p, true)).GetEnumerator();
        }

        protected override ISwDmCustomProperty CreatePropertyInstance(string name, bool isCreated)
            => new SwDmCutListCustomProperty(m_CutList, name, isCreated);

        protected override bool Exists(string name)
            => (m_CutList.CutListItem.GetCustomPropertyNames() as string[])?
            .Contains(name, StringComparer.CurrentCultureIgnoreCase) == true;
    }

    internal class SwDmCutListCustomProperty : SwDmCustomProperty
    {
        private readonly ISwDmCutListItem m_CutList;

        public SwDmCutListCustomProperty(ISwDmCutListItem cutList, string name, bool isCreated) 
            : base(name, isCreated)
        {
            m_CutList = cutList;
        }

        protected override void AddValue(object value)
        {
            SwDmCustomInfoType type = GetPropertyType(value);

            if (!m_CutList.CutListItem.AddCustomProperty(Name, type, value?.ToString()))
            {
                throw new Exception("Failed to add custom property");
            }
        }

        protected override object ReadValue(out string exp)
        {
            //TODO: parse type

            var val = m_CutList.CutListItem.GetCustomPropertyValue2(Name, out SwDmCustomInfoType type, out exp);

            if (string.IsNullOrEmpty(exp)) 
            {
                exp = val;
            }

            return val;
        }

        protected override void SetValue(object value) 
            => m_CutList.CutListItem.SetCustomProperty(Name, value?.ToString());

        internal override void Delete()
        {
            if (!m_CutList.CutListItem.DeleteCustomProperty(Name))
            {
                throw new Exception("Failed to delete property");
            }
        }
    }
}

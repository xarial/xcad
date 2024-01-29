//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
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

namespace Xarial.XCad.SwDocumentManager.Data
{
    internal class SwDmConfigurationCustomPropertiesCollection : SwDmCustomPropertiesCollection
    {
        public override int Count => m_Conf.Configuration.GetCustomPropertyCount();

        private readonly SwDmConfiguration m_Conf;

        internal SwDmConfigurationCustomPropertiesCollection(SwDmConfiguration conf)
        {
            m_Conf = conf;
        }

        public override IEnumerator<IXProperty> GetEnumerator()
        {
            var prpNames = m_Conf.Configuration.GetCustomPropertyNames() as string[] ?? new string[0];
            prpNames = prpNames.Except(new string[] { SwDmConfiguration.QTY_PROPERTY }).ToArray();

            return prpNames.Select(p => CreatePropertyInstance(p, true)).GetEnumerator();
        }

        protected override ISwDmCustomProperty CreatePropertyInstance(string name, bool isCreated)
            => new SwDmConfigurationCustomProperty(m_Conf, name, isCreated);

        protected override bool Exists(string name)
            => (m_Conf.Configuration.GetCustomPropertyNames() as string[])?
            .Contains(name, StringComparer.CurrentCultureIgnoreCase) == true;
    }

    internal class SwDmConfigurationCustomProperty : SwDmCustomProperty
    {
        private readonly SwDmConfiguration m_Conf;

        internal SwDmConfigurationCustomProperty(SwDmConfiguration conf, string name, bool isCreated) 
            : base(name, isCreated)
        {
            m_Conf = conf;
        }

        protected override void AddValue(object value)
        {
            SwDmCustomInfoType type = GetPropertyType(value);

            if (!m_Conf.Configuration.AddCustomProperty(Name, type, value?.ToString()))
            {
                throw new Exception("Failed to add custom property");
            }

            m_Conf.Document.IsDirty = true;
        }

        protected override string ReadRawValue(out SwDmCustomInfoType type, out string linkedTo)
            => ((ISwDMConfiguration5)m_Conf.Configuration).GetCustomPropertyValues(Name, out type, out linkedTo);

        protected override void SetValue(object value)
        {
            m_Conf.Configuration.SetCustomProperty(Name, value?.ToString());
            m_Conf.Document.IsDirty = true;
        }

        internal override void Delete()
        {
            if (!m_Conf.Configuration.DeleteCustomProperty(Name))
            {
                throw new Exception("Failed to delete property");
            }

            m_Conf.Document.IsDirty = true;
        }
    }
}

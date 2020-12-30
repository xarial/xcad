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

        private readonly ISwDmConfiguration m_Conf;

        internal SwDmConfigurationCustomPropertiesCollection(ISwDmConfiguration conf)
        {
            m_Conf = conf;
        }

        public override IEnumerator<IXProperty> GetEnumerator()
        {
            var prpNames = m_Conf.Configuration.GetCustomPropertyNames() as string[] ?? new string[0];
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
        private readonly ISwDmConfiguration m_Conf;

        public SwDmConfigurationCustomProperty(ISwDmConfiguration conf, string name, bool isCreated) 
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
        }

        protected override object ReadValue()
        {
            //TODO: parse type
            return m_Conf.Configuration.GetCustomProperty(Name, out SwDmCustomInfoType type);
        }

        protected override void SetValue(object value) 
            => m_Conf.Configuration.SetCustomProperty(Name, value?.ToString());

        internal override void Delete()
        {
            if (!m_Conf.Configuration.DeleteCustomProperty(Name))
            {
                throw new Exception("Failed to delete property");
            }
        }
    }
}

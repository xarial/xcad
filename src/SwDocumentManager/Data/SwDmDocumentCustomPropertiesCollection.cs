//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Data;
using Xarial.XCad.SwDocumentManager.Documents;
using System.Linq;
using SolidWorks.Interop.swdocumentmgr;

namespace Xarial.XCad.SwDocumentManager.Data
{
    internal class SwDmDocumentCustomPropertiesCollection : SwDmCustomPropertiesCollection
    {
        public override int Count => m_Doc.Document.GetCustomPropertyCount();

        private readonly SwDmDocument m_Doc;

        internal SwDmDocumentCustomPropertiesCollection(SwDmDocument doc) : base(doc)
        {
            m_Doc = doc;
        }

        public override IEnumerator<IXProperty> GetEnumerator()
        {
            var prpNames = m_Doc.Document.GetCustomPropertyNames() as string[] ?? new string[0];
            prpNames = prpNames.Except(new string[] { SwDmConfiguration.QTY_PROPERTY }).ToArray();
            return prpNames.Select(p => CreatePropertyInstance(p, true)).GetEnumerator();
        }

        protected override ISwDmCustomProperty CreatePropertyInstance(string name, bool isCreated)
            => new SwDmDocumentCustomProperty(m_Doc, name, isCreated);

        protected override bool Exists(string name) 
            => (m_Doc.Document.GetCustomPropertyNames() as string[])?
            .Contains(name, StringComparer.CurrentCultureIgnoreCase) == true;
    }

    internal class SwDmDocumentCustomProperty : SwDmCustomProperty
    {
        private readonly ISwDmDocument m_Doc;
        
        public SwDmDocumentCustomProperty(SwDmDocument doc, string name, bool isCreated) : base(name, isCreated, doc, doc.OwnerApplication)
        {
            m_Doc = doc;
        }

        protected override void AddValue(object value)
        {
            SwDmCustomInfoType type = GetPropertyType(value);

            if (!m_Doc.Document.AddCustomProperty(Name, type, value?.ToString()))
            {
                throw new Exception("Failed to add custom property");
            }

            m_Doc.IsDirty = true;
        }

        protected override string ReadRawValue(out SwDmCustomInfoType type, out string linkedTo)
            => ((ISwDMDocument5)m_Doc.Document).GetCustomPropertyValues(Name, out type, out linkedTo);

        protected override void SetValue(object value)
        {
            m_Doc.Document.SetCustomProperty(Name, value?.ToString());
            m_Doc.IsDirty = true;
        }

        internal override void Delete() 
        {
            if (!m_Doc.Document.DeleteCustomProperty(Name)) 
            {
                throw new Exception("Failed to delete property");
            }

            m_Doc.IsDirty = true;
        }
    }
}

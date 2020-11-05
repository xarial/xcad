using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Services
{
    public class LazyNewDocumentGeometryBuilderDocumentProvider : IMemoryGeometryBuilderDocumentProvider
    {
        protected readonly ISwApplication m_App;

        private ISwDocument m_TempDoc;

        public LazyNewDocumentGeometryBuilderDocumentProvider(ISwApplication app)
        {
            m_App = app;
        }

        public ISwDocument ProvideDocument(Type geomType) => GetTempDocument();

        protected virtual ISwDocument CreateTempDocument()
        {
            var activeDoc = m_App.Documents.Active;

            var doc = (SwDocument)m_App.Documents.NewPart();
            doc.Title = "xCADGeometryBuilderDoc_" + Guid.NewGuid().ToString();
            
            if (activeDoc != null) 
            {
                m_App.Documents.Active = activeDoc;
            }

            doc.Visible = false;

            return doc;
        }

        private ISwDocument GetTempDocument()
        {
            if (!IsTempDocAlive())
            {
                m_TempDoc = CreateTempDocument();
            }

            return m_TempDoc;
        }

        private bool IsTempDocAlive()
        {
            if (m_TempDoc != null)
            {
                try
                {
                    var title = m_TempDoc.Title;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}

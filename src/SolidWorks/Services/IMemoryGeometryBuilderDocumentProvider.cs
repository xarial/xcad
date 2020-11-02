using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Services
{
    public interface IMemoryGeometryBuilderDocumentProvider
    {
        SwDocument ProvideDocument(Type geomType);
    }

    internal class DefaultMemoryGeometryBuilderDocumentProvider : IMemoryGeometryBuilderDocumentProvider
    {
        private readonly SwApplication m_App;

        internal DefaultMemoryGeometryBuilderDocumentProvider(SwApplication app)
        {
            m_App = app;
        }

        public SwDocument ProvideDocument(Type geomType)
        {
            var part = m_App.Documents.Active as SwPart;

            if (part == null)
            {
                part = m_App.Documents.OfType<SwPart>().FirstOrDefault();
            }

            if (part == null)
            {
                throw new Exception("Failed to find part document for memory geometry builder");
            }

            return part;
        }
    }
}

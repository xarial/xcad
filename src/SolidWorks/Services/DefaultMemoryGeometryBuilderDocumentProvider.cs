using System;
using System.Linq;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Services
{
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

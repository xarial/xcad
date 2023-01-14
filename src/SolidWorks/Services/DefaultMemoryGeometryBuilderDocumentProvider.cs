//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Linq;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Services
{
    internal class DefaultMemoryGeometryBuilderDocumentProvider : IMemoryGeometryBuilderDocumentProvider
    {
        private readonly ISwApplication m_App;

        internal DefaultMemoryGeometryBuilderDocumentProvider(ISwApplication app)
        {
            m_App = app;
        }

        public ISwDocument ProvideDocument(Type geomType)
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

using System;
using Xarial.XCad;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Extensions;

namespace __TemplateNamePlaceholder__Console.Base
{
    public class Controller
    {
        private readonly IXApplication m_App;

        public Controller(IXApplication app) 
        {
            m_App = app;
        }

        public void PrintProperties(string filePath) 
        {
            using (var doc = m_App.Documents.Open(filePath, DocumentState_e.ReadOnly)) 
            {
                OutputProperties(doc.Properties);

                if (doc is IXDocument3D) 
                {
                    foreach (var conf in ((IXDocument3D)doc).Configurations) 
                    {
                        OutputProperties(conf.Properties);
                    }
                }
            }
        }

        private void OutputProperties(IXPropertyRepository prps)
        {
            throw new NotImplementedException();
        }
    }
}

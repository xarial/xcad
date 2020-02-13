using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Services;

namespace SwAddInExample
{
    public class SwDocHandler : IDocumentHandler
    {
        private IXApplication m_App;
        private IXDocument m_Model;

        public void Init(IXApplication app, IXDocument model)
        {
            m_App = app;
            m_Model = model;
            m_App.ShowMessageBox($"Opened {model.Title}");
        }

        public void Dispose()
        {
            m_App.ShowMessageBox($"Closed {m_Model.Title}");
        }
    }
}

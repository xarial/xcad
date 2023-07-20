using System;
using System.Runtime.InteropServices;
using Xarial.XCad;
using Xarial.XCad.Annotations;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Extensions;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.SolidWorks;

namespace Xarial.XCad.Documentation
{
    //--- EventHandlers
    public class MyDocumentHandler : IDocumentHandler
    {
        private IXDocument m_Model;
        private IXProperty m_DescPrp;
        private IXDimension m_D1Dim;

        public void Init(IXApplication app, IXDocument model)
        {
            m_Model = model;

            m_DescPrp = m_Model.Properties["Description"];
            m_D1Dim = m_Model.Dimensions["D1@Sketch1"];

            m_Model.Closing += OnModelClosing;
            m_Model.Selections.NewSelection += OnNewSelection;
            
            m_DescPrp.ValueChanged += OnPropertyValueChanged;
            m_D1Dim.ValueChanged += OnDimensionValueChanged;
        }

        private void OnModelClosing(IXDocument doc, DocumentCloseType_e type)
        {
            //handle closing
        }

        private void OnNewSelection(IXDocument doc, IXSelObject selObject)
        {
            //handle new selection
        }

        private void OnPropertyValueChanged(IXProperty prp, object newValue)
        {
            //handle property change
        }

        private void OnDimensionValueChanged(IXDimension dim, double newVal)
        {
            //handle dimension change
        }

        public void Dispose()
        {
            m_Model.Closing -= OnModelClosing;
            m_Model.Selections.NewSelection -= OnNewSelection;

            m_DescPrp.ValueChanged -= OnPropertyValueChanged;
            m_D1Dim.ValueChanged -= OnDimensionValueChanged;
        }
    }
    //---

    [ComVisible(true), Guid("A57F10A3-D23F-40C9-92DA-D4AE92E4FE8C")]
    public class EventsAddIn : SwAddInEx    
    {   
        public override void OnConnect()
        {
            //--- RegisterHandler
            Application.Documents.RegisterHandler<MyDocumentHandler>();
            //---
        }
    }
}

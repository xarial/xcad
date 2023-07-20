using SolidWorks.Interop.sldworks;
using System;
using System.Runtime.InteropServices;
using Xarial.XCad;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documents.Extensions;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Documents.Services;

namespace Xarial.XCad.Documentation
{
    [Title("Document Manager Add-In")]
    [ComVisible(true), Guid("37728E73-BB74-4430-9FEB-AEC2B97DF861")]
    public class DocMgrAddIn : SwAddInEx    
    {
        //--- DocHandlerDefinition
        public class MyDocHandler : SwDocumentHandler
        {
            protected override void AttachPartEvents(PartDoc part)
            {
                part.AddItemNotify += OnAddItemNotify;
            }

            protected override void DetachPartEvents(PartDoc part)
            {
                part.AddItemNotify -= OnAddItemNotify;
            }

            private int OnAddItemNotify(int EntityType, string itemName)
            {
                //Implement
                return 0;
            }
        }
        //---

        //--- DocHandlerInit
        public override void OnConnect()
        {
            this.Application.Documents.RegisterHandler<MyDocHandler>();
        }
        //---
    }
}

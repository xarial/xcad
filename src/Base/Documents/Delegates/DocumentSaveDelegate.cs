using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Structures;

namespace Xarial.XCad.Documents.Delegates
{
    public delegate void DocumentSaveDelegate(IXDocument doc, DocumentSaveType_e type, DocumentSaveArgs args);
}

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
}

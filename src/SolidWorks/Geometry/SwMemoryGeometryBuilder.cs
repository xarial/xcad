using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Services;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public class SwMemoryGeometryBuilder : IXGeometryBuilder
    {
        public IXWireGeometryBuilder WireBuilder { get; }
        public IXSheetGeometryBuilder SheetBuilder { get; }
        public IXSolidGeometryBuilder SolidBuilder { get; }

        internal SwMemoryGeometryBuilder(SwApplication app, IMemoryGeometryBuilderDocumentProvider geomBuilderDocsProvider) 
        {
            var mathUtils = app.Sw.IGetMathUtility();
            var modeler = app.Sw.IGetModeler();

            WireBuilder = new SwMemoryWireGeometryBuilder(mathUtils, modeler);
            SheetBuilder = new SwMemorySheetGeometryBuilder(mathUtils, modeler);
            SolidBuilder = new SwMemorySolidGeometryBuilder(app, geomBuilderDocsProvider);
        }
    }
}

//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Toolkit.Data;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwMemoryGeometryBuilder : IXMemoryGeometryBuilder
    {
    }

    internal class SwMemoryGeometryBuilder : ISwMemoryGeometryBuilder
    {
        public IXWireGeometryBuilder WireBuilder { get; }
        public IXSheetGeometryBuilder SheetBuilder { get; }
        public IXSolidGeometryBuilder SolidBuilder { get; }

        private readonly IModeler m_Modeler;

        internal SwMemoryGeometryBuilder(ISwApplication app, IMemoryGeometryBuilderDocumentProvider geomBuilderDocsProvider) 
        {
            var mathUtils = app.Sw.IGetMathUtility();
            m_Modeler = app.Sw.IGetModeler();

            WireBuilder = new SwMemoryWireGeometryBuilder(mathUtils, m_Modeler);
            SheetBuilder = new SwMemorySheetGeometryBuilder(mathUtils, m_Modeler);
            SolidBuilder = new SwMemorySolidGeometryBuilder(app, geomBuilderDocsProvider);
        }

        public IXBody DeserializeBody(Stream stream)
        {
            var comStr = new StreamWrapper(stream);
            var body = (IBody2)m_Modeler.Restore(comStr);
            return SwObjectFactory.FromDispatch<ISwTempBody>(body, null);
        }

        public void SerializeBody(IXBody body, Stream stream)
        {
            var comStr = new StreamWrapper(stream);
            ((SwBody)body).Body.Save(comStr);
        }
    }
}

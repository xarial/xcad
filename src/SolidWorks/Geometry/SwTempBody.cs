//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Primitives;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwTempBody : ISwBody, IXMemoryBody
    {
    }

    internal class SwTempBody : SwBody, ISwTempBody
    {
        private IBody2 m_TempBody;

        public override IBody2 Body => m_TempBody;
        public override object Dispatch => m_TempBody;

        //NOTE: keeping the pointer in this class only so it can be properly disposed
        internal SwTempBody(IBody2 body, SwApplication app) : base(null, null, app)
        {
            if (!body.IsTemporaryBody()) 
            {
                throw new ArgumentException("Body is not temp");
            }

            m_TempBody = body;
        }

        public override ISwBody CreateResilient()
            => throw new NotSupportedException("Only permanent bodies can be converter to resilient bodies");

        public void Preview(IXDocument3D doc, Color color)
        {
            if (doc is ISwPart)
            {
                Preview((ISwPart)doc, color, false);
            }
            else 
            {
                throw new NotSupportedException();
            }
        }

        private void Preview(ISwPart part, Color color, bool selectable)
        {
            var opts = selectable
                ? swTempBodySelectOptions_e.swTempBodySelectable
                : swTempBodySelectOptions_e.swTempBodySelectOptionNone;

            Body.Display3(part.Model, ColorUtils.ToColorRef(color), (int)opts);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Marshal.FinalReleaseComObject(m_TempBody);
            }

            m_TempBody = null;
        }
    }

    public interface ISwTempSolidBody : ISwTempBody, ISwSolidBody, IXMemorySolidBody
    {
    }

    public interface ISwTempSheetBody : ISwTempBody, ISwSheetBody, IXMemorySheetBody
    {
    }

    public interface ISwTempPlanarSheetBody : ISwTempSheetBody, ISwPlanarSheetBody, ISwTempRegion, IXMemoryPlanarSheetBody
    {
    }

    public interface ISwTempWireBody : ISwTempBody, ISwWireBody, IXMemoryWireBody
    {
    }
    
    internal class SwTempSolidBody : SwTempBody, ISwTempSolidBody
    {
        internal SwTempSolidBody(IBody2 body, SwApplication app) : base(body, app)
        {
        }

        public double Volume => this.GetVolume();
    }

    internal class SwTempSheetBody : SwTempBody, ISwTempSheetBody
    {
        internal SwTempSheetBody(IBody2 body, SwApplication app) : base(body, app)
        {
        }
    }

    internal class SwTempPlanarSheetBody : SwTempBody, ISwTempPlanarSheetBody
    {
        IXLoop[] IXRegion.Boundary => Boundary;

        internal SwTempPlanarSheetBody(IBody2 body, SwApplication app) : base(body, app)
        {
        }

        public Plane Plane => this.GetPlane();

        public ISwTempPlanarSheetBody PlanarSheetBody => this;

        public ISwLoop[] Boundary => this.GetBoundary();
    }

    internal class SwTempWireBody : SwTempBody, ISwTempWireBody
    {
        internal SwTempWireBody(IBody2 body, SwApplication app) : base(body, app)
        {
        }
    }
}
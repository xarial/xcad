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
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Primitives;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwTempBody : ISwBody, IDisposable
    {
        void Preview(ISwPart part, Color color, bool selectable = false);
    }

    internal class SwTempBody : SwBody, ISwTempBody
    {
        private IBody2 m_TempBody;

        public override IBody2 Body => m_TempBody;
        public override object Dispatch => m_TempBody;

        private readonly IMathUtility m_MathUtils;

        //NOTE: keeping the pointer in this class only so it can be properly disposed
        internal SwTempBody(IBody2 body, ISwApplication app) : base(null, null, app)
        {
            if (!body.IsTemporaryBody()) 
            {
                throw new ArgumentException("Body is not temp");
            }

            m_TempBody = body;
            m_MathUtils = app.Sw.IGetMathUtility();
        }

        public void Preview(ISwPart part, Color color, bool selectable = false) 
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

        public override ISwBody CreateResilient()
            => throw new NotSupportedException("Only permanent bodies can be converter to resilient bodies");
    }

    public interface ISwTempSolidBody : ISwTempBody, ISwSolidBody
    {
    }

    public interface ISwTempSheetBody : ISwTempBody, ISwSheetBody 
    {
    }

    public interface ISwTempPlanarSheetBody : ISwTempSheetBody, ISwPlanarSheetBody, ISwTempRegion
    {
    }

    public interface ISwTempWireBody : ISwTempBody, ISwWireBody
    {
    }
    
    internal class SwTempSolidBody : SwTempBody, ISwTempSolidBody
    {
        internal SwTempSolidBody(IBody2 body, ISwApplication app) : base(body, app)
        {
        }

        public double Volume => this.GetVolume();
    }

    internal class SwTempSheetBody : SwTempBody, ISwTempSheetBody
    {
        internal SwTempSheetBody(IBody2 body, ISwApplication app) : base(body, app)
        {
        }
    }

    internal class SwTempPlanarSheetBody : SwTempBody, ISwTempPlanarSheetBody
    {
        internal SwTempPlanarSheetBody(IBody2 body, ISwApplication app) : base(body, app)
        {
        }

        public Plane Plane => this.GetPlane();

        public IXSegment[] Boundary => this.GetBoundary();

        public ISwTempPlanarSheetBody PlanarSheetBody => this;

        ISwCurve[] ISwRegion.Boundary => this.GetBoundary();
    }

    internal class SwTempWireBody : SwTempBody, ISwTempWireBody
    {
        internal SwTempWireBody(IBody2 body, ISwApplication app) : base(body, app)
        {
        }
    }
}
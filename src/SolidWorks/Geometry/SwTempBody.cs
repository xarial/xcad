//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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

        //NOTE: keeping the pointer in this class only so it can be properly disposed

        private readonly Lazy<SwMathUtilsProvider> m_MathUtilsProvider;

        internal SwTempBody(IBody2 body) : base(null, null)
        {
            if (!body.IsTemporaryBody()) 
            {
                throw new ArgumentException("Body is not temp");
            }

            m_TempBody = body;
            m_MathUtilsProvider = new Lazy<SwMathUtilsProvider>(() => new SwMathUtilsProvider(this));
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

        public override void Move(TransformMatrix transform)
        {
            var mathTransform = (MathTransform)m_MathUtilsProvider.Value.CreateTransform(transform);
            
            if (!Body.ApplyTransform(mathTransform))
            {
                throw new Exception("Failed to apply transform to the body");
            }
        }
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

    internal class SwTempSolidBody : SwTempBody, ISwTempSolidBody
    {
        internal SwTempSolidBody(IBody2 body) : base(body)
        {
        }

        public double Volume => this.GetVolume();
    }

    internal class SwTempSheetBody : SwTempBody, ISwTempSheetBody
    {
        internal SwTempSheetBody(IBody2 body) : base(body)
        {
        }
    }

    internal class SwTempPlanarSheetBody : SwTempBody, ISwTempPlanarSheetBody
    {
        internal SwTempPlanarSheetBody(IBody2 body) : base(body)
        {
        }

        public Plane Plane => this.GetPlane();

        public IXSegment[] Boundary => this.GetBoundary();

        public ISwTempPlanarSheetBody PlanarSheetBody => this;

        ISwCurve[] ISwRegion.Boundary => this.GetBoundary();
    }
}
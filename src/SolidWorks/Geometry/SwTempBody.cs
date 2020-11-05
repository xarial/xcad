//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwTempBody : ISwBody, IDisposable
    {
    }

    public class SwTempBody : SwBody, ISwTempBody
    {
        private IBody2 m_TempBody;

        public override IBody2 Body => m_TempBody;
        public override object Dispatch => m_TempBody;

        //NOTE: keeping the pointer in this class only so it can be properly disposed

        internal SwTempBody(IBody2 body) : base(null, null)
        {
            if (!body.IsTemporaryBody()) 
            {
                throw new ArgumentException("Body is not temp");
            }

            m_TempBody = body;
        }

        public void Preview(SwPart part, Color color, bool selectable = false) 
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

    public interface ISwTempSolidBody : ISwTempBody, ISwSolidBody
    {
    }

    public interface ISwTempSheetBody : ISwTempBody, ISwSheetBody 
    {
    }

    public interface ISwTempPlanarSheetBody : ISwTempSheetBody, ISwPlanarSheetBody, ISwTempRegion
    {
    }

    public class SwTempSolidBody : SwTempBody, ISwTempSolidBody
    {
        internal SwTempSolidBody(IBody2 body) : base(body)
        {
        }
    }

    public class SwTempSheetBody : SwTempBody, ISwTempSheetBody
    {
        internal SwTempSheetBody(IBody2 body) : base(body)
        {
        }
    }

    public class SwTempPlanarSheetBody : SwTempBody, ISwTempPlanarSheetBody
    {
        internal SwTempPlanarSheetBody(IBody2 body) : base(body)
        {
        }

        public Plane Plane => this.GetPlane();

        public IXSegment[] Boundary => this.GetBoundary();

        SwTempPlanarSheetBody ISwTempRegion.Body => this;

        SwCurve[] ISwRegion.Boundary => this.GetBoundary();
    }
}
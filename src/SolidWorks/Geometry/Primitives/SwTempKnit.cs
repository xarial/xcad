using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Primitives;

namespace Xarial.XCad.SolidWorks.Geometry.Primitives
{
    public interface ISwTempKnit : IXKnit, ISwTempPrimitive
    {
        new ISwFace[] Faces { get; set; }
    }

    public interface ISwTempSurfaceKnit : ISwTempKnit, ISwTempPrimitive
    {
        new ISwFace[] Faces { get; set; }
        new ISwTempSheetBody[] Bodies { get; }
    }

    public interface ISwTempSolidKnit : ISwTempKnit, ISwTempPrimitive
    {
        new ISwFace[] Faces { get; set; }
        new ISwTempSolidBody[] Bodies { get; }
    }

    internal abstract class SwTempKnit : SwTempPrimitive, ISwTempKnit 
    {
        IXFace[] IXKnit.Faces { get => Faces; set => Faces = value.Cast<ISwFace>().ToArray(); }

        internal SwTempKnit(SwTempBody[] bodies, ISwApplication app, bool isCreated) : base(bodies, app, isCreated)
        {
        }

        public ISwFace[] Faces
        {
            get => m_Creator.CachedProperties.Get<ISwFace[]>();
            set
            {
                if (IsCommitted)
                {
                    throw new Exception("Cannot change faces for the commited temp knit");
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        protected override ISwTempBody[] CreateBodies(CancellationToken cancellationToken)
        {
            if (Faces?.Length > 0)
            {
                var sheetBodies = Faces.Select(f => f.Face.ICreateSheetBody()).ToArray();

                var tol = 1E-16;

                int errs = -1;

                var bodies = (object[])m_Modeler.CreateBodiesFromSheets2(sheetBodies, (int)SewingType, tol, ref errs);

                if (bodies != null)
                {
                    return bodies.Select(b => m_App.CreateObjectFromDispatch<ISwTempBody>((IBody2)b, null)).ToArray();
                }
                else
                {
                    throw new Exception($"Failed to knit surfaces into a body. Error code: {(swSheetSewingError_e)errs} ({errs})");
                }
            }
            else
            {
                throw new Exception("No faces specified for knitting");
            }
        }

        protected abstract swSheetSewingOption_e SewingType { get; }
    }

    internal class SwTempSurfaceKnit : SwTempKnit, ISwTempSurfaceKnit
    {
        ISwTempSheetBody[] ISwTempSurfaceKnit.Bodies => (ISwTempSheetBody[])base.Bodies;

        protected override swSheetSewingOption_e SewingType => swSheetSewingOption_e.swSewToSheets;

        internal SwTempSurfaceKnit(SwTempBody[] bodies, ISwApplication app, bool isCreated) : base(bodies, app, isCreated)
        {
        }
    }

    internal class SwTempSolidKnit : SwTempKnit, ISwTempSolidKnit
    {
        ISwTempSolidBody[] ISwTempSolidKnit.Bodies => (ISwTempSolidBody[])base.Bodies;

        protected override swSheetSewingOption_e SewingType => swSheetSewingOption_e.swSewToSolid;

        internal SwTempSolidKnit(SwTempBody[] bodies, ISwApplication app, bool isCreated) : base(bodies, app, isCreated)
        {
        }
    }
}

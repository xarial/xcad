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
using System.Linq;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Primitives;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwBody : IXBody
    {
        IBody2 Body { get; }
    }

    public class SwBody : SwSelObject, ISwBody
    {
        public static SwBody operator -(SwBody firstBody, SwBody secondBody)
        {
            return (SwBody)firstBody.Substract(secondBody).First();
        }

        public static SwBody operator +(SwBody firstBody, SwBody secondBody)
        {
            return (SwBody)firstBody.Add(secondBody);
        }

        public virtual IBody2 Body { get; }

        public bool Visible
        {
            get => Body.Visible;
            set
            {
                Body.HideBody(!value);
            }
        }

        public string Name => Body.Name;

        private IComponent2 Component => (Body.IGetFirstFace() as IEntity)?.GetComponent() as IComponent2;

        public Color? Color
        {
            get => SwColorHelper.FromMaterialProperties(Body.MaterialPropertyValues2 as double[]);
            set
            {
                if (value.HasValue)
                {
                    var matPrps = SwColorHelper.ToMaterialProperties(value.Value);
                    Body.MaterialPropertyValues2 = matPrps;
                }
                else 
                {
                    SwColorHelper.GetColorScope(Component, 
                        out swInConfigurationOpts_e confOpts, out string[] confs);

                    Body.RemoveMaterialProperty((int)confOpts, confs);
                }
            }
        }

        protected SwDocument m_Document;

        internal SwBody(IBody2 body, SwDocument doc) : base(doc?.Model, body)
        {
            Body = body;
            m_Document = doc;
        }

        public IXBody Add(IXBody other)
        {
            return PerformOperation(other, swBodyOperationType_e.SWBODYADD).FirstOrDefault();
        }

        public IXBody[] Substract(IXBody other)
        {
            return PerformOperation(other, swBodyOperationType_e.SWBODYCUT);
        }

        public IXBody[] Common(IXBody other)
        {
            return PerformOperation(other, swBodyOperationType_e.SWBODYINTERSECT);
        }

        public SwTempBody ToTempBody()
        {
            return FromDispatch<SwTempBody>(Body.ICopy());
        }

        private IXBody[] PerformOperation(IXBody other, swBodyOperationType_e op)
        {
            if (other is SwBody)
            {
                var thisBody = Body;
                var otherBody = (other as SwBody).Body;

                int errs;
                var res = thisBody.Operations2((int)op, otherBody, out errs) as object[];

                if (errs != (int)swBodyOperationError_e.swBodyOperationNoError)
                {
                    throw new Exception($"Body boolean operation failed: {(swBodyOperationError_e)errs}");
                }

                if (res?.Any() == true)
                {
                    return res.Select(b => FromDispatch<SwBody>(b as IBody2)).ToArray();
                }
                else
                {
                    return new IXBody[0];
                }
            }
            else
            {
                throw new InvalidCastException();
            }
        }

        public override void Select(bool append)
        {
            if (!Body.Select2(append, null))
            {
                throw new Exception("Failed to select body");
            }
        }
    }

    public interface ISwSheetBody : ISwBody, IXSheetBody
    {
    }

    public class SwSheetBody : SwBody, ISwSheetBody
    {
        internal SwSheetBody(IBody2 body, SwDocument doc) : base(body, doc)
        {
        }
    }

    public interface ISwPlanarSheetBody : ISwSheetBody, IXPlanarSheetBody
    {
    }

    public class SwPlanarSheetBody : SwSheetBody, ISwPlanarSheetBody
    {
        internal SwPlanarSheetBody(IBody2 body, SwDocument doc) : base(body, doc)
        {
        }

        public Plane Plane => this.GetPlane();
        public IXSegment[] Boundary => this.GetBoundary();
    }

    internal static class ISwPlanarSheetBodyExtension 
    {
        internal static Plane GetPlane(this ISwPlanarSheetBody body)
        {
            var face = body.Body.IGetFirstFace();
            var surf = face.IGetSurface();
            var planeParams = surf.PlaneParams as double[];

            var rootPt = new XCad.Geometry.Structures.Point(planeParams[3], planeParams[4], planeParams[5]);
            var normVec = new Vector(planeParams[0], planeParams[1], planeParams[2]);
            var refVec = normVec.CreateAnyPerpendicular();

            return new Plane(rootPt, normVec, refVec);
        }

        internal static SwCurve[] GetBoundary(this ISwPlanarSheetBody body)
        {
            var face = body.Body.IGetFirstFace();
            var edges = face.GetEdges() as object[];
            var segs = new SwCurve[edges.Length];

            for (int i = 0; i < segs.Length; i++)
            {
                var curve = (edges[i] as IEdge).IGetCurve();
                segs[i] = SwSelObject.FromDispatch<SwCurve>(curve);
            }

            return segs;
        }
    }

    public interface ISwSolidBody : ISwBody
    {
    }

    public class SwSolidBody : SwBody, ISwBody, ISwSolidBody
    {
        internal SwSolidBody(IBody2 body, SwDocument doc) : base(body, doc)
        {
        }
    }
}
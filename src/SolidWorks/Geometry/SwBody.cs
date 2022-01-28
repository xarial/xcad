//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
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
    public interface ISwBody : ISwSelObject, IXBody, IResilientibleObject<ISwBody>
    {
        IBody2 Body { get; }

        ISwBody Add(ISwBody other);
        ISwBody[] Substract(ISwBody other);
        ISwBody[] Common(ISwBody other);
    }

    internal class SwBody : SwSelObject, ISwBody
    {
        IXBody IXBody.Add(IXBody other) => Add((ISwBody)other);
        IXBody[] IXBody.Substract(IXBody other) => Substract((ISwBody)other);
        IXBody[] IXBody.Common(IXBody other) => Common((ISwBody)other);

        IXObject IResilientibleObject.CreateResilient() => CreateResilient();

        public virtual IBody2 Body 
        {
            get 
            {
                if (IsResilient)
                {
                    try
                    {
                        var testPtrAlive = m_Body.Name;
                    }
                    catch 
                    {
                        var body = (IBody2)OwnerDocument.Model.Extension.GetObjectByPersistReference3(m_PersistId, out _);

                        if (body != null)
                        {
                            m_Body = body;
                        }
                        else 
                        {
                            throw new NullReferenceException("Pointer to the body cannot be restored");
                        }
                    }
                }

                return m_Body;
            }
        }

        public override object Dispatch => Body;

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

        public IEnumerable<IXFace> Faces 
        {
            get 
            {
                var face = Body.IGetFirstFace();

                while (face != null) 
                {
                    yield return OwnerApplication.CreateObjectFromDispatch<ISwFace>(face, OwnerDocument);
                    face = face.IGetNextFace();
                }
            }
        }

        public IEnumerable<IXEdge> Edges
        {
            get
            {
                var edges = Body.GetEdges() as object[];

                if (edges != null) 
                {
                    foreach (IEdge edge in edges) 
                    {
                        yield return OwnerApplication.CreateObjectFromDispatch<ISwEdge>(edge, OwnerDocument);
                    }
                }
            }
        }

        public bool IsResilient { get; private set; }

        private byte[] m_PersistId;

        private IBody2 m_Body;

        internal SwBody(IBody2 body, ISwDocument doc, ISwApplication app) 
            : base(body, doc, app ?? ((SwDocument)doc)?.OwnerApplication)
        {
            m_Body = body;
        }

        public override void Select(bool append)
        {
            if (!Body.Select2(append, null))
            {
                throw new Exception("Failed to select body");
            }
        }

        public ISwBody Add(ISwBody other)
            => PerformOperation(other, swBodyOperationType_e.SWBODYADD).FirstOrDefault();

        public ISwBody[] Substract(ISwBody other)
            => PerformOperation(other, swBodyOperationType_e.SWBODYCUT);

        public ISwBody[] Common(ISwBody other)=>
            PerformOperation(other, swBodyOperationType_e.SWBODYINTERSECT);

        private ISwBody[] PerformOperation(ISwBody other, swBodyOperationType_e op)
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
                return res.Select(b => OwnerApplication.CreateObjectFromDispatch<SwBody>(b as IBody2, OwnerDocument)).ToArray();
            }
            else
            {
                return new ISwBody[0];
            }
        }

        public IXBody Copy()
            => OwnerApplication.CreateObjectFromDispatch<SwTempBody>(Body.ICopy(), OwnerDocument);

        public virtual void Transform(TransformMatrix transform)
            => throw new NotSupportedException($"Only temp bodies are supported. Use {nameof(Copy)} method");

        public virtual ISwBody CreateResilient()
        {
            if (OwnerDocument == null) 
            {
                throw new NullReferenceException("Owner document is not set");
            }

            var id = (byte[])OwnerDocument.Model.Extension.GetPersistReference3(Body);

            if (id != null)
            {
                var body = OwnerDocument.CreateObjectFromDispatch<SwBody>(Body);
                body.MakeResilient(id);
                return body;
            }
            else 
            {
                throw new Exception("Failed to create resilient body");
            }
        }

        private void MakeResilient(byte[] persistId) 
        {
            IsResilient = true;
            m_PersistId = persistId;
        }
    }

    public interface ISwSheetBody : ISwBody, IXSheetBody
    {
    }

    internal class SwSheetBody : SwBody, ISwSheetBody
    {
        internal SwSheetBody(IBody2 body, ISwDocument doc, ISwApplication app) : base(body, doc, app)
        {
        }
    }

    public interface ISwPlanarSheetBody : ISwSheetBody, IXPlanarSheetBody
    {
    }

    internal class SwPlanarSheetBody : SwSheetBody, ISwPlanarSheetBody
    {
        internal SwPlanarSheetBody(IBody2 body, ISwDocument doc, ISwApplication app) : base(body, doc, app)
        {
        }

        public Plane Plane => this.GetPlane();
        public IXSegment[] Boundary => this.GetBoundary();
    }

    internal static class ISwPlanarSheetBodyExtension 
    {
        internal static Plane GetPlane(this ISwPlanarSheetBody body)
        {
            var planarFace = ((SwObject)body).OwnerApplication.CreateObjectFromDispatch<SwPlanarFace>(
                body.Body.IGetFirstFace(), ((SwObject)body).OwnerDocument);

            return planarFace.Definition.Plane;
        }

        internal static SwCurve[] GetBoundary(this ISwPlanarSheetBody body)
        {
            var face = body.Body.IGetFirstFace();
            var edges = face.GetEdges() as object[];
            var segs = new SwCurve[edges.Length];

            for (int i = 0; i < segs.Length; i++)
            {
                var curve = ((IEdge)edges[i]).IGetCurve();
                segs[i] = ((SwObject)body).OwnerApplication.CreateObjectFromDispatch<SwCurve>(curve, ((SwObject)body).OwnerDocument);
            }

            return segs;
        }
    }

    public interface ISwSolidBody : IXSolidBody, ISwBody
    {
    }

    internal class SwSolidBody : SwBody, ISwBody, ISwSolidBody
    {
        internal SwSolidBody(IBody2 body, ISwDocument doc, ISwApplication app) : base(body, doc, app)
        {
        }

        public double Volume => this.GetVolume();
    }

    internal static class ISwBodyExtension
    { 
        public static double GetVolume(this ISwBody body)
        {
            var massPrps = body.Body.GetMassProperties(0) as double[];
            return massPrps[3];
        }
    }

    public interface ISwWireBody : ISwBody, IXWireBody 
    {
    }

    internal class SwWireBody : SwBody, ISwWireBody
    {
        internal SwWireBody(IBody2 body, ISwDocument doc, ISwApplication app) : base(body, doc, app)
        {
        }
    }
}
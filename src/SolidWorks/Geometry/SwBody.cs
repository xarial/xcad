//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
using Xarial.XCad.Geometry.Exceptions;
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

        private readonly IMathUtility m_MathUtils;

        internal SwBody(IBody2 body, SwDocument doc, SwApplication app)
            : base(body, doc, app ?? ((SwDocument)doc)?.OwnerApplication)
        {
            m_Body = body;
            m_MathUtils = app.Sw.IGetMathUtility();
        }

        internal override void Select(bool append, ISelectData selData)
        {
            if (!Body.Select2(append, (SelectData)selData))
            {
                throw new Exception("Failed to select body");
            }
        }

        public ISwBody Add(ISwBody other)
        {
            var res = PerformOperation(other, swBodyOperationType_e.SWBODYADD);

            if (res.Length == 0) 
            {
                throw new Exception("No bodies are created as the result of this operation");
            }

            if (res.Length > 1) 
            {
                throw new BodyBooleanOperationNoIntersectException();
            }

            return res.First();
        }

        /// <remarks>Empty array can be returned if bodies are equal</remarks>
        public ISwBody[] Substract(ISwBody other)
            => PerformOperation(other, swBodyOperationType_e.SWBODYCUT);

        public ISwBody[] Common(ISwBody other)
        {
            var res = PerformOperation(other, swBodyOperationType_e.SWBODYINTERSECT);

            if (!res.Any()) 
            {
                throw new BodyBooleanOperationNoIntersectException();
            }

            return res;
        }
        
        private ISwBody[] PerformOperation(ISwBody other, swBodyOperationType_e op)
        {
            var thisBody = Body;
            var otherBody = (other as SwBody).Body;

            int errs;
            var res = thisBody.Operations2((int)op, otherBody, out errs) as object[];

            if (errs == (int)swBodyOperationError_e.swBodyOperationNonManifold)
            {
                //NOTE: as per the SOLIDWORKS API documentation resetting the edge tolerances and trying again
                otherBody.ResetEdgeTolerances();
                thisBody.ResetEdgeTolerances();

                res = thisBody.Operations2((int)op, otherBody, out errs) as object[];
            }

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

        public void Transform(TransformMatrix transform)
        {
            var mathTransform = (MathTransform)m_MathUtils.ToMathTransform(transform);

            if (!Body.ApplyTransform(mathTransform))
            {
                if (!Body.IsTemporaryBody())
                {
                    throw new NotSupportedException($"Only temp bodies or bodies within the context of macro feature regeneration are supported. Use {nameof(Copy)} method");
                }
                else 
                {
                    throw new Exception("Failed to apply transform to the body"); 
                }
            }
        }

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
        internal SwSheetBody(IBody2 body, SwDocument doc, SwApplication app) : base(body, doc, app)
        {
        }
    }

    public interface ISwPlanarSheetBody : ISwSheetBody, IXPlanarSheetBody
    {
    }

    internal class SwPlanarSheetBody : SwSheetBody, ISwPlanarSheetBody
    {
        internal SwPlanarSheetBody(IBody2 body, SwDocument doc, SwApplication app) : base(body, doc, app)
        {
        }

        public Plane Plane => this.GetPlane();
        public IXLoop[] Boundary => this.GetBoundary();
    }

    internal static class ISwPlanarSheetBodyExtension 
    {
        internal static Plane GetPlane(this ISwPlanarSheetBody body)
        {
            var planarFace = ((SwObject)body).OwnerApplication.CreateObjectFromDispatch<SwPlanarFace>(
                body.Body.IGetFirstFace(), ((SwObject)body).OwnerDocument);

            return planarFace.Definition.Plane;
        }

        internal static SwLoop[] GetBoundary(this ISwPlanarSheetBody body)
        {
            var face = body.Body.IGetFirstFace();

            var loops = (object[])face.GetLoops();

            var res = new SwLoop[loops.Length];

            for (int i = 0; i < loops.Length; i++)
            {
                var loop = (ILoop2)loops[i];
                res[i] = ((SwObject)body).OwnerApplication.CreateObjectFromDispatch<SwLoop>(loop, ((SwObject)body).OwnerDocument);
            }

            return res;
        }
    }

    public interface ISwSolidBody : IXSolidBody, ISwBody
    {
    }

    internal class SwSolidBody : SwBody, ISwBody, ISwSolidBody
    {
        internal SwSolidBody(IBody2 body, SwDocument doc, SwApplication app) : base(body, doc, app)
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
        internal SwWireBody(IBody2 body, SwDocument doc, SwApplication app) : base(body, doc, app)
        {
        }
    }
}
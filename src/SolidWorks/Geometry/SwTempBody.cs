//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Xarial.XCad.Documents;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Exceptions;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Primitives;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Geometry
{
    /// <summary>
    /// Represents SOLIDWORKS specific memory body
    /// </summary>
    public interface ISwTempBody : ISwBody, IXMemoryBody
    {
        ISwTempBody Add(ISwTempBody other);
        ISwTempBody[] Subtract(ISwTempBody other);
        ISwTempBody[] Common(ISwTempBody other);
    }

    /// <summary>
    /// Utility class to perform operations related to temp bodies
    /// </summary>
    /// <remarks>This is implemented as a separate class as bodies used in macro feature can perform the operations of temp body,
    /// while remain the non temp bodies</remarks>
    internal class SwTempBodyContainer : IDisposable
    {
        private enum DisplayBodyResult_e
        {
            Success = 0,
            NotTempBody = 1,
            InvalidComponent = 2,
            NotPart = 3
        }

        private object m_CurrentPreviewContext;

        internal IBody2 Body => m_Body;

        //NOTE: keeping the pointer in this class only so it can be properly disposed
        private IBody2 m_Body;

        private readonly IMathUtility m_MathUtils;

        protected readonly SwDocument m_Doc;
        protected readonly SwApplication m_App;

        internal SwTempBodyContainer(IBody2 body, SwDocument doc, SwApplication app)
        {
            m_Doc = doc;
            m_App = app;
            m_Body = body;
            m_MathUtils = app.Sw.IGetMathUtility();
        }

        internal virtual void Preview(IXObject context, Color color, bool selectable)
        {
            switch (context)
            {
                case ISwPart part:
                    Display(Body, part.Model, color, selectable);
                    break;

                case ISwComponent comp:
                    Display(Body, comp.Component, color, selectable);
                    break;

                default:
                    throw new NotSupportedException("Only ISwPart or ISwComponent is supported as the context");
            }
        }

        internal void HidePreview() 
        {
            if (m_CurrentPreviewContext != null)
            {
                Body.Hide(m_CurrentPreviewContext);
                m_CurrentPreviewContext = null;
            }
            else
            {
                throw new NotSupportedException("Body was not previewed");
            }
        }

        internal virtual ISwTempBody Add(ISwTempBody other)
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
        internal virtual ISwTempBody[] Subtract(ISwTempBody other)
            => PerformOperation(other, swBodyOperationType_e.SWBODYCUT);

        internal virtual ISwTempBody[] Common(ISwTempBody other)
        {
            var res = PerformOperation(other, swBodyOperationType_e.SWBODYINTERSECT);

            if (!res.Any())
            {
                throw new BodyBooleanOperationNoIntersectException();
            }

            return res;
        }

        internal virtual void Transform(TransformMatrix transform)
        {
            var mathTransform = (MathTransform)m_MathUtils.ToMathTransform(transform);

            if (!Body.ApplyTransform(mathTransform))
            {
                if (!Body.IsTemporaryBody())
                {
                    throw new NotSupportedException($"Only temp bodies or bodies within the context of macro feature regeneration are supported. Use {nameof(IXBody.Copy)} method");
                }
                else
                {
                    throw new Exception("Failed to apply transform to the body");
                }
            }
        }

        private void Display(IBody2 body, object context, Color color, bool selectable)
        {
            var opts = selectable
                ? swTempBodySelectOptions_e.swTempBodySelectable
                : swTempBodySelectOptions_e.swTempBodySelectOptionNone;

            var res = (DisplayBodyResult_e)body.Display3(context, ColorUtils.ToColorRef(color), (int)opts);

            if (res != DisplayBodyResult_e.Success)
            {
                throw new Exception($"Failed to render preview body: {res}");
            }

            var hasAlpha = color.A < 255;

            if (hasAlpha)
            {
                //COLORREF does not encode alpha channel, so assigning the color via material properties
                var matPrps = SwColorHelper.ToMaterialProperties(color);
                Body.MaterialPropertyValues2 = matPrps;
            }

            m_CurrentPreviewContext = context;
        }

        private ISwTempBody[] PerformOperation(ISwTempBody other, swBodyOperationType_e op)
        {
            var thisBody = Body;
            var otherBody = other.Body;

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
                return res.Select(b =>
                {
                    var body = (IBody2)b;

                    return CreateBodyInstance(body);

                }).ToArray();
            }
            else
            {
                return Array.Empty<ISwTempBody>();
            }
        }

        protected virtual ISwTempBody CreateBodyInstance(IBody2 body) 
            => m_App.CreateObjectFromDispatch<SwTempBody>(body, m_Doc);

        public virtual void Dispose()
        {
            if (m_Body != null)
            {
                Marshal.FinalReleaseComObject(m_Body);
            }

            m_Body = null;
        }
    }

    [DebuggerDisplay("<Temp Body>")]
    internal class SwTempBody : SwBody, ISwTempBody
    {
        IXMemoryBody IXMemoryBody.Add(IXMemoryBody other) => Add((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Subtract(IXMemoryBody other) => Subtract((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Common(IXMemoryBody other) => Common((ISwTempBody)other);

        public override IBody2 Body => m_Creator.Element.Body;
        public override object Dispatch => Body;

        protected readonly ElementCreator<SwTempBodyContainer> m_Creator;

        public override bool IsCommitted => m_Creator.IsCreated;

        internal SwTempBody(IBody2 body, SwApplication app) : base(null, null, app)
        {
            m_Creator = new ElementCreator<SwTempBodyContainer>(CreateBodyContainer, 
                body != null ? new SwTempBodyContainer(body, null, app) : null, body != null);

            if (body != null && !body.IsTemporaryBody()) 
            {
                throw new ArgumentException("Body is not temp");
            }
        }

        public override void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);

        private SwTempBodyContainer CreateBodyContainer(CancellationToken cancellationToken)
            => new SwTempBodyContainer(CreateTempBody(cancellationToken), null, OwnerApplication);

        protected virtual IBody2 CreateTempBody(CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public override ISwBody CreateResilient()
            => throw new NotSupportedException("Only permanent bodies can be converter to resilient bodies");

        public override bool Visible
        {
            get => Body.Visible;
            set
            {
                if (!value)
                {
                    m_Creator.Element.HidePreview();
                }
                else 
                {
                    throw new NotSupportedException($"Use {nameof(Preview)} method to show hide body");
                }
            }
        }

        public override string Name 
        {
            get => throw new NotSupportedException("Temp body does not support name"); 
            set => throw new NotSupportedException("Temp body does not support name");
        }

        public void Preview(IXObject context, Color color, bool selectable) => m_Creator.Element.Preview(context, color, selectable);

        public ISwTempBody Add(ISwTempBody other) => m_Creator.Element.Add(other);

        /// <remarks>Empty array can be returned if bodies are equal</remarks>
        public ISwTempBody[] Subtract(ISwTempBody other) => m_Creator.Element.Subtract(other);

        public ISwTempBody[] Common(ISwTempBody other) => m_Creator.Element.Common(other);

        public void Transform(TransformMatrix transform) => m_Creator.Element.Transform(transform);

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_Creator.IsCreated)
                {
                    m_Creator.Element.Dispose();
                }
            }
        }
    }

    public interface ISwTempSolidBody : ISwTempBody, ISwSolidBody, IXMemorySolidBody
    {
    }

    public interface ISwTempSheetBody : ISwTempBody, ISwSheetBody, IXMemorySheetBody
    {
    }

    public interface ISwTempPlanarSheetBody : ISwTempSheetBody, ISwPlanarSheetBody, ISwPlanarRegion, IXMemoryPlanarSheetBody
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
        IXLoop IXRegion.OuterLoop { get => OuterLoop; set => throw new NotSupportedException(); }
        IXLoop[] IXRegion.InnerLoops { get => InnerLoops; set => throw new NotSupportedException(); }

        internal SwTempPlanarSheetBody(IBody2 body, SwApplication app) : base(body, app)
        {
        }

        public Plane Plane => this.GetPlane();

        public ISwTempPlanarSheetBody PlanarSheetBody => this;

        public ISwLoop OuterLoop { get => this.GetOuterLoop(); set => throw new NotSupportedException(); }
        public ISwLoop[] InnerLoops { get => this.GetInnerLoops(); set => throw new NotSupportedException(); }
    }

    internal class SwTempWireBody : SwTempBody, ISwTempWireBody
    {
        public IXSegment[] Segments 
        {
            get 
            {
                if (IsCommitted)
                {
                    return Edges.ToArray();
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<IXSegment[]>();
                }
            }
            set 
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        internal SwTempWireBody(IBody2 body, SwApplication app) : base(body, app)
        {
            if (body != null) 
            {
                if (body.GetType() != (int)swBodyType_e.swWireBody) 
                {
                    throw new Exception("Specified body is not a wire body");
                }
            }
        }

        protected override IBody2 CreateTempBody(CancellationToken cancellationToken)
        {
            var curves = Segments.SelectMany(s => s.GetCurve().Curves).ToArray();

            if (!curves.Any())
            {
                throw new Exception("No curves found");
            }

            IBody2 wireBody;

            if (curves.Length == 1)
            {
                wireBody = curves.First().CreateWireBody();
            }
            else 
            {
                wireBody = OwnerApplication.Sw.IGetModeler().CreateWireBody(curves, (int)swCreateWireBodyOptions_e.swCreateWireBodyByDefault);
            }
            
            if (wireBody == null)
            {
                throw new NullReferenceException($"Wire body cannot be created from the curves");
            }

            return wireBody;
        }
    }
}
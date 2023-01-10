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
using System.Linq;
using System.Runtime.InteropServices;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Exceptions;
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
        ISwTempBody Add(ISwTempBody other);
        ISwTempBody[] Substract(ISwTempBody other);
        ISwTempBody[] Common(ISwTempBody other);
    }

    internal static class SwTempBodyHelper 
    {
        internal static ISwTempBody Add(IBody2 thisBody, IBody2 otherBody,
            SwApplication ownerApp, SwDocument ownerDoc, Func<IBody2, ISwTempBody> customInstCreator = null)
        {
            var res = PerformOperation(thisBody, otherBody, swBodyOperationType_e.SWBODYADD, ownerApp, ownerDoc, customInstCreator);

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

        internal static ISwTempBody[] Substract(IBody2 thisBody, IBody2 otherBody,
            SwApplication ownerApp, SwDocument ownerDoc, Func<IBody2, ISwTempBody> customInstCreator = null)
            => PerformOperation(thisBody, otherBody, swBodyOperationType_e.SWBODYCUT, ownerApp, ownerDoc, customInstCreator);

        internal static ISwTempBody[] Common(IBody2 thisBody, IBody2 otherBody,
            SwApplication ownerApp, SwDocument ownerDoc, Func<IBody2, ISwTempBody> customInstCreator = null)
        {
            var res = PerformOperation(thisBody, otherBody, swBodyOperationType_e.SWBODYINTERSECT, ownerApp, ownerDoc, customInstCreator);

            if (!res.Any())
            {
                throw new BodyBooleanOperationNoIntersectException();
            }

            return res;
        }

        private static ISwTempBody[] PerformOperation(IBody2 thisBody, IBody2 otherBody, swBodyOperationType_e op, 
            SwApplication ownerApp, SwDocument ownerDoc, Func<IBody2, ISwTempBody> customInstCreator)
        {
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

                    if (customInstCreator != null)
                    {
                        return customInstCreator.Invoke(body);
                    }
                    else 
                    {
                        return ownerApp.CreateObjectFromDispatch<SwTempBody>(body, ownerDoc);
                    }
                }).ToArray();
            }
            else
            {
                return new ISwTempBody[0];
            }
        }
    }

    internal class SwTempBody : SwBody, ISwTempBody
    {
        IXMemoryBody IXMemoryBody.Add(IXMemoryBody other) => Add((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Substract(IXMemoryBody other) => Substract((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Common(IXMemoryBody other) => Common((ISwTempBody)other);

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

        public override bool Visible
        {
            get => Body.Visible;
            set
            {
                if (!value)
                {
                    if (m_CurrentPreviewContext != null)
                    {
                        Body.Hide(m_CurrentPreviewContext.Part);
                        m_CurrentPreviewContext = null;
                    }
                    else 
                    {
                        throw new NotSupportedException("Body was not previewed");
                    }
                }
                else 
                {
                    throw new NotSupportedException($"Use {nameof(Preview)} method to show hide body");
                }
            }
        }

        private ISwPart m_CurrentPreviewContext;

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

            m_CurrentPreviewContext = part;
        }

        public ISwTempBody Add(ISwTempBody other)
            => SwTempBodyHelper.Add(Body, ((SwBody)other).Body, OwnerApplication, OwnerDocument);

        /// <remarks>Empty array can be returned if bodies are equal</remarks>
        public ISwTempBody[] Substract(ISwTempBody other)
            => SwTempBodyHelper.Substract(Body, ((SwBody)other).Body, OwnerApplication, OwnerDocument);

        public ISwTempBody[] Common(ISwTempBody other)
            => SwTempBodyHelper.Common(Body, ((SwBody)other).Body, OwnerApplication, OwnerDocument);

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
        internal SwTempWireBody(IBody2 body, SwApplication app) : base(body, app)
        {
        }
    }
}
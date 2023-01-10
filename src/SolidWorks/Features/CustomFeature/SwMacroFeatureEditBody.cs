using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.SolidWorks.Geometry;

namespace Xarial.XCad.SolidWorks.Features.CustomFeature
{
    public interface ISwMacroFeatureEditBody : ISwTempBody
    {
        bool IsPreviewMode { get; }
    }

    internal static class SwMacroFeatureEditBody
    {
        internal static ISwMacroFeatureEditBody CreateMacroFeatureEditBody(IBody2 body, SwApplication app, bool isPreview)
        {
            var bodyType = (swBodyType_e)body.GetType();

            switch (bodyType)
            {
                case swBodyType_e.swSheetBody:
                    if (body.GetFaceCount() == 1 && body.IGetFirstFace().IGetSurface().IsPlane())
                    {
                        return new SwPlanarSheetMacroFeatureEditBody(body, app, isPreview);
                    }
                    else
                    {
                        return new SwSheetMacroFeatureEditBody(body, app, isPreview);
                    }

                case swBodyType_e.swSolidBody:
                    return new SwSolidMacroFeatureEditBody(body, app, isPreview);

                case swBodyType_e.swWireBody:
                    return new SwWireMacroFeatureEditBody(body, app, isPreview);

                default:
                    throw new NotSupportedException();
            }
        }

        internal static IBody2 ProvideBooleanOperationBody(this ISwMacroFeatureEditBody editBody, IBody2 thisBody) 
        {
            if (editBody.IsPreviewMode)
            {
                return thisBody.ICopy();
            }
            else 
            {
                return thisBody;
            }
        }
    }

    internal class SwPlanarSheetMacroFeatureEditBody : SwTempPlanarSheetBody, ISwMacroFeatureEditBody
    {
        public bool IsPreviewMode { get; }

        internal SwPlanarSheetMacroFeatureEditBody(IBody2 body, SwApplication app, bool isPreview) : base(body, app)
        {
            IsPreviewMode = isPreview;
        }

        protected override ISwTempBody[] PerformOperation(IBody2 thisBody, IBody2 other, swBodyOperationType_e op)
            => base.PerformOperation(this.ProvideBooleanOperationBody(thisBody), other, op);
    }

    internal class SwSheetMacroFeatureEditBody : SwTempSheetBody, ISwMacroFeatureEditBody
    {
        public bool IsPreviewMode { get; }

        internal SwSheetMacroFeatureEditBody(IBody2 body, SwApplication app, bool isPreview) : base(body, app)
        {
            IsPreviewMode = isPreview;
        }

        protected override ISwTempBody[] PerformOperation(IBody2 thisBody, IBody2 other, swBodyOperationType_e op)
            => base.PerformOperation(this.ProvideBooleanOperationBody(thisBody), other, op);
    }

    internal class SwSolidMacroFeatureEditBody : SwTempSolidBody, ISwMacroFeatureEditBody
    {
        public bool IsPreviewMode { get; }

        internal SwSolidMacroFeatureEditBody(IBody2 body, SwApplication app, bool isPreview) : base(body, app)
        {
            IsPreviewMode = isPreview;
        }

        protected override ISwTempBody[] PerformOperation(IBody2 thisBody, IBody2 other, swBodyOperationType_e op)
            => base.PerformOperation(this.ProvideBooleanOperationBody(thisBody), other, op);
    }

    internal class SwWireMacroFeatureEditBody : SwTempWireBody, ISwMacroFeatureEditBody
    {
        public bool IsPreviewMode { get; }

        internal SwWireMacroFeatureEditBody(IBody2 body, SwApplication app, bool isPreview) : base(body, app)
        {
            IsPreviewMode = isPreview;
        }

        protected override ISwTempBody[] PerformOperation(IBody2 thisBody, IBody2 other, swBodyOperationType_e op)
            => base.PerformOperation(this.ProvideBooleanOperationBody(thisBody), other, op);
    }
}

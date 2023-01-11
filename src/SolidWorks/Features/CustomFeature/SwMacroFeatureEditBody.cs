using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry;

namespace Xarial.XCad.SolidWorks.Features.CustomFeature
{
    public interface ISwMacroFeatureEditBody : ISwTempBody
    {
        bool IsPreviewMode { get; }
    }

    internal static class SwMacroFeatureEditBody
    {
        internal static ISwMacroFeatureEditBody CreateMacroFeatureEditBody(IBody2 body, SwDocument doc, SwApplication app, bool isPreview)
        {
            var bodyType = (swBodyType_e)body.GetType();

            switch (bodyType)
            {
                case swBodyType_e.swSheetBody:
                    if (body.GetFaceCount() == 1 && body.IGetFirstFace().IGetSurface().IsPlane())
                    {
                        return new SwPlanarSheetMacroFeatureEditBody(body, doc, app, isPreview);
                    }
                    else
                    {
                        return new SwSheetMacroFeatureEditBody(body, doc, app, isPreview);
                    }

                case swBodyType_e.swSolidBody:
                    return new SwSolidMacroFeatureEditBody(body, doc, app, isPreview);

                case swBodyType_e.swWireBody:
                    return new SwWireMacroFeatureEditBody(body, doc, app, isPreview);

                default:
                    throw new NotSupportedException();
            }
        }

        internal static ISwTempBody PerformAdd(this ISwMacroFeatureEditBody editBody, ISwTempBody other)
            => SwTempBodyHelper.Add(ProvideBooleanOperationBody(editBody), ((SwBody)other).Body, (SwApplication)editBody.OwnerApplication, (SwDocument)editBody.OwnerDocument,
                b => CreateTempBodyBooleanOperationResult(b, (SwDocument)editBody.OwnerDocument, (SwApplication)editBody.OwnerApplication, editBody.IsPreviewMode));

        internal static ISwTempBody[] PerformSubstract(this ISwMacroFeatureEditBody editBody, ISwTempBody other)
            => SwTempBodyHelper.Substract(ProvideBooleanOperationBody(editBody), ((SwBody)other).Body, (SwApplication)editBody.OwnerApplication, (SwDocument)editBody.OwnerDocument,
                b => CreateTempBodyBooleanOperationResult(b, (SwDocument)editBody.OwnerDocument, (SwApplication)editBody.OwnerApplication, editBody.IsPreviewMode));

        internal static ISwTempBody[] PerformCommon(this ISwMacroFeatureEditBody editBody, ISwTempBody other)
            => SwTempBodyHelper.Common(ProvideBooleanOperationBody(editBody), ((SwBody)other).Body, (SwApplication)editBody.OwnerApplication, (SwDocument)editBody.OwnerDocument,
                b => CreateTempBodyBooleanOperationResult(b, (SwDocument)editBody.OwnerDocument, (SwApplication)editBody.OwnerApplication, editBody.IsPreviewMode));

        private static ISwTempBody CreateTempBodyBooleanOperationResult(IBody2 body, SwDocument doc, SwApplication app, bool isPreview) 
        {
            if (body.IsTemporaryBody())
            {
                return doc.CreateObjectFromDispatch<ISwTempBody>(body);
            }
            else 
            {
                return CreateMacroFeatureEditBody(body, doc, app, isPreview);
            }
        }

        private static IBody2 ProvideBooleanOperationBody(ISwMacroFeatureEditBody editBody) 
        {
            if (editBody.IsPreviewMode)
            {
                return editBody.Body.ICopy();
            }
            else 
            {
                return editBody.Body;
            }
        }
    }

    internal class SwPlanarSheetMacroFeatureEditBody : SwPlanarSheetBody, ISwMacroFeatureEditBody
    {
        IXMemoryBody IXMemoryBody.Add(IXMemoryBody other) => Add((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Substract(IXMemoryBody other) => Substract((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Common(IXMemoryBody other) => Common((ISwTempBody)other);

        public bool IsPreviewMode { get; }

        internal SwPlanarSheetMacroFeatureEditBody(IBody2 body, SwDocument doc, SwApplication app, bool isPreview) : base(body, doc, app)
        {
            IsPreviewMode = isPreview;
        }

        public ISwTempBody Add(ISwTempBody other) => this.PerformAdd(other);
        public ISwTempBody[] Substract(ISwTempBody other) => this.PerformSubstract(other);
        public ISwTempBody[] Common(ISwTempBody other) => this.PerformCommon(other);

        public void Preview(IXObject context, Color color)
        {
        }

        public void Dispose()
        {
        }
    }

    internal class SwSheetMacroFeatureEditBody : SwSheetBody, ISwMacroFeatureEditBody
    {
        IXMemoryBody IXMemoryBody.Add(IXMemoryBody other) => Add((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Substract(IXMemoryBody other) => Substract((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Common(IXMemoryBody other) => Common((ISwTempBody)other);

        public bool IsPreviewMode { get; }

        internal SwSheetMacroFeatureEditBody(IBody2 body, SwDocument doc, SwApplication app, bool isPreview) : base(body, doc, app)
        {
            IsPreviewMode = isPreview;
        }

        public ISwTempBody Add(ISwTempBody other) => this.PerformAdd(other);
        public ISwTempBody[] Substract(ISwTempBody other) => this.PerformSubstract(other);
        public ISwTempBody[] Common(ISwTempBody other) => this.PerformCommon(other);

        public void Preview(IXObject context, Color color)
        {
        }

        public void Dispose()
        {
        }
    }

    internal class SwSolidMacroFeatureEditBody : SwSolidBody, ISwMacroFeatureEditBody
    {
        IXMemoryBody IXMemoryBody.Add(IXMemoryBody other) => Add((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Substract(IXMemoryBody other) => Substract((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Common(IXMemoryBody other) => Common((ISwTempBody)other);

        public bool IsPreviewMode { get; }

        internal SwSolidMacroFeatureEditBody(IBody2 body, SwDocument doc, SwApplication app, bool isPreview) : base(body, doc, app)
        {
            IsPreviewMode = isPreview;
        }

        public ISwTempBody Add(ISwTempBody other) => this.PerformAdd(other);
        public ISwTempBody[] Substract(ISwTempBody other) => this.PerformSubstract(other);
        public ISwTempBody[] Common(ISwTempBody other) => this.PerformCommon(other);

        public void Preview(IXObject context, Color color)
        {
        }

        public void Dispose()
        {
        }
    }

    internal class SwWireMacroFeatureEditBody : SwWireBody, ISwMacroFeatureEditBody
    {
        IXMemoryBody IXMemoryBody.Add(IXMemoryBody other) => Add((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Substract(IXMemoryBody other) => Substract((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Common(IXMemoryBody other) => Common((ISwTempBody)other);

        public bool IsPreviewMode { get; }

        internal SwWireMacroFeatureEditBody(IBody2 body, SwDocument doc, SwApplication app, bool isPreview) : base(body, doc, app)
        {
            IsPreviewMode = isPreview;
        }

        public ISwTempBody Add(ISwTempBody other) => this.PerformAdd(other);
        public ISwTempBody[] Substract(ISwTempBody other) => this.PerformSubstract(other);
        public ISwTempBody[] Common(ISwTempBody other) => this.PerformCommon(other);

        public void Preview(IXObject context, Color color)
        {
        }

        public void Dispose()
        {
        }
    }
}

//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Features.CustomFeature
{
    /// <summary>
    /// Edit body represents the body which is edited in the macro feature (decorated with <see cref="XCad.Features.CustomFeature.Attributes.ParameterEditBodyAttribute"/>)
    /// Use <see cref="SwMacroFeatureDefinition.CreateEditBody(IBody2, ISwDocument, ISwApplication, bool)"/> to create an instance of edit body
    /// </summary>
    /// <remarks>Body used in the regeneration is mix of the temp body and real body (e.g. it supports boolean operations with temp bodies)
    /// For the cosistent access in the preview <see cref="ISwMacroFeatureEditBody"/> is added which represents both temp body (in preview mode) and permanent body in the regeneration</remarks>
    internal interface ISwMacroFeatureEditBody : ISwTempBody
    {
        IBody2 PreviewBody { get; }
        bool IsPreviewMode { get; }
    }

    internal static class SwMacroFeatureEditBody
    {
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
                return (ISwTempBody)SwMacroFeatureDefinition.CreateEditBody(body, doc, app, isPreview);
            }
        }

        private static IBody2 ProvideBooleanOperationBody(ISwMacroFeatureEditBody editBody) 
        {
            if (editBody.IsPreviewMode)
            {
                return editBody.PreviewBody;
            }
            else 
            {
                return editBody.Body;
            }
        }
    }

    internal class LazyMacroFeaturePreviewBody : Lazy<IBody2> 
    {
        internal LazyMacroFeaturePreviewBody(IBody2 body, bool isPreview, SwApplication app) 
            : base(() => 
            {
                if (isPreview)
                {
                    return body.CreateCopy(app);
                }
                else 
                {
                    throw new Exception("Macro feature body is not in a preview state");
                }
            })
        {

        }
    }

    internal class SwPlanarSheetMacroFeatureEditBody : SwPlanarSheetBody, ISwMacroFeatureEditBody, ISwTempPlanarSheetBody
    {
        IXMemoryBody IXMemoryBody.Add(IXMemoryBody other) => Add((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Substract(IXMemoryBody other) => Substract((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Common(IXMemoryBody other) => Common((ISwTempBody)other);

        public bool IsPreviewMode { get; }

        public IBody2 PreviewBody => m_PreviewBodyLazy.Value;

        private Lazy<IBody2> m_PreviewBodyLazy;

        internal SwPlanarSheetMacroFeatureEditBody(IBody2 body, SwDocument doc, SwApplication app, bool isPreview) : base(body, doc, app)
        {
            IsPreviewMode = isPreview;
            m_PreviewBodyLazy = new LazyMacroFeaturePreviewBody(body, isPreview, app);
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

    internal class SwSheetMacroFeatureEditBody : SwSheetBody, ISwMacroFeatureEditBody, ISwTempSheetBody
    {
        IXMemoryBody IXMemoryBody.Add(IXMemoryBody other) => Add((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Substract(IXMemoryBody other) => Substract((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Common(IXMemoryBody other) => Common((ISwTempBody)other);

        public bool IsPreviewMode { get; }

        public IBody2 PreviewBody => m_PreviewBodyLazy.Value;

        private Lazy<IBody2> m_PreviewBodyLazy;

        internal SwSheetMacroFeatureEditBody(IBody2 body, SwDocument doc, SwApplication app, bool isPreview) : base(body, doc, app)
        {
            IsPreviewMode = isPreview;
            m_PreviewBodyLazy = new LazyMacroFeaturePreviewBody(body, isPreview, app);
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

    internal class SwSolidMacroFeatureEditBody : SwSolidBody, ISwMacroFeatureEditBody, ISwTempSolidBody
    {
        IXMemoryBody IXMemoryBody.Add(IXMemoryBody other) => Add((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Substract(IXMemoryBody other) => Substract((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Common(IXMemoryBody other) => Common((ISwTempBody)other);

        public bool IsPreviewMode { get; }

        public IBody2 PreviewBody => m_PreviewBodyLazy.Value;

        private Lazy<IBody2> m_PreviewBodyLazy;

        internal SwSolidMacroFeatureEditBody(IBody2 body, SwDocument doc, SwApplication app, bool isPreview) : base(body, doc, app)
        {
            IsPreviewMode = isPreview;
            m_PreviewBodyLazy = new LazyMacroFeaturePreviewBody(body, isPreview, app);
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

    internal class SwWireMacroFeatureEditBody : SwWireBody, ISwMacroFeatureEditBody, ISwTempWireBody
    {
        IXMemoryBody IXMemoryBody.Add(IXMemoryBody other) => Add((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Substract(IXMemoryBody other) => Substract((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Common(IXMemoryBody other) => Common((ISwTempBody)other);

        public bool IsPreviewMode { get; }

        public IBody2 PreviewBody => m_PreviewBodyLazy.Value;

        private Lazy<IBody2> m_PreviewBodyLazy;

        internal SwWireMacroFeatureEditBody(IBody2 body, SwDocument doc, SwApplication app, bool isPreview) : base(body, doc, app)
        {
            IsPreviewMode = isPreview;
            m_PreviewBodyLazy = new LazyMacroFeaturePreviewBody(body, isPreview, app);
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

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
using Xarial.XCad.Geometry.Structures;
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
        ISwTempBody PreviewBody { get; }
        bool IsPreviewMode { get; }
    }

    internal class SwMacroFeatureBodyContainer : SwTempBodyContainer
    {
        private readonly bool m_IsPreview;

        internal Lazy<ISwTempBody> PreviewBody { get; }

        internal SwMacroFeatureBodyContainer(IBody2 body, SwDocument doc, SwApplication app, bool isPreview)
            : base(body, doc, app)
        {
            m_IsPreview = isPreview;

            PreviewBody = new Lazy<ISwTempBody>(() => app.CreateObjectFromDispatch<ISwTempBody>(body.CreateCopy(app), null));
        }

        internal override ISwTempBody Add(ISwTempBody other)
        {
            if (m_IsPreview)
            {
                return PreviewBody.Value.Add(other);
            }
            else
            {
                return base.Add(other);
            }
        }

        internal override ISwTempBody[] Common(ISwTempBody other)
        {
            if (m_IsPreview)
            {
                return PreviewBody.Value.Common(other);
            }
            else
            {
                return base.Common(other);
            }
        }

        internal override ISwTempBody[] Subtract(ISwTempBody other)
        {
            if (m_IsPreview)
            {
                return PreviewBody.Value.Subtract(other);
            }
            else
            {
                return base.Subtract(other);
            }
        }

        internal override void Transform(TransformMatrix transform)
        {
            if (m_IsPreview)
            {
                PreviewBody.Value.Transform(transform);
            }
            else
            {
                base.Transform(transform);
            }
        }

        internal override void Preview(IXObject context, Color color, bool selectable)
        {
            if (m_IsPreview)
            {
                PreviewBody.Value.Preview(context, color, selectable);
            }
            else
            {
                throw new Exception("Preview is only supported in the preview mode");
            }
        }

        protected override ISwTempBody CreateBodyInstance(IBody2 body)
            => SwMacroFeatureDefinition.CreateEditBody(body, m_Doc, m_App, m_IsPreview);

        public override void Dispose()
        {
            //do not dispose macro feature body as it is not a temp body, only dispose preview body

            if (PreviewBody.IsValueCreated)
            {
                PreviewBody.Value.Dispose();
            }
        }
    }

    internal class SwPlanarSheetMacroFeatureEditBody : SwPlanarSheetBody, ISwMacroFeatureEditBody, ISwTempPlanarSheetBody
    {
        IXMemoryBody IXMemoryBody.Add(IXMemoryBody other) => Add((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Subtract(IXMemoryBody other) => Subtract((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Common(IXMemoryBody other) => Common((ISwTempBody)other);

        public bool IsPreviewMode { get; }

        public ISwTempBody PreviewBody => m_MacroFeatureBodyContainer.PreviewBody.Value;

        private readonly SwMacroFeatureBodyContainer m_MacroFeatureBodyContainer;

        internal SwPlanarSheetMacroFeatureEditBody(IBody2 body, SwDocument doc, SwApplication app, bool isPreview) : base(body, doc, app)
        {
            IsPreviewMode = isPreview;
            m_MacroFeatureBodyContainer = new SwMacroFeatureBodyContainer(body, doc, app, isPreview);
        }

        public void Preview(IXObject context, Color color, bool selectable) => m_MacroFeatureBodyContainer.Preview(context, color, selectable);
        public ISwTempBody Add(ISwTempBody other) => m_MacroFeatureBodyContainer.Add(other);
        public ISwTempBody[] Subtract(ISwTempBody other) => m_MacroFeatureBodyContainer.Subtract(other);
        public ISwTempBody[] Common(ISwTempBody other) => m_MacroFeatureBodyContainer.Common(other);
        public void Transform(TransformMatrix transform) => m_MacroFeatureBodyContainer.Transform(transform);
        public void Dispose() => m_MacroFeatureBodyContainer.Dispose();
    }

    internal class SwSheetMacroFeatureEditBody : SwSheetBody, ISwMacroFeatureEditBody, ISwTempSheetBody
    {
        IXMemoryBody IXMemoryBody.Add(IXMemoryBody other) => Add((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Subtract(IXMemoryBody other) => Subtract((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Common(IXMemoryBody other) => Common((ISwTempBody)other);

        public bool IsPreviewMode { get; }

        public ISwTempBody PreviewBody => m_MacroFeatureBodyContainer.PreviewBody.Value;

        private readonly SwMacroFeatureBodyContainer m_MacroFeatureBodyContainer;

        internal SwSheetMacroFeatureEditBody(IBody2 body, SwDocument doc, SwApplication app, bool isPreview) : base(body, doc, app)
        {
            IsPreviewMode = isPreview;
            m_MacroFeatureBodyContainer = new SwMacroFeatureBodyContainer(body, doc, app, isPreview);
        }

        public void Preview(IXObject context, Color color, bool selectable) => m_MacroFeatureBodyContainer.Preview(context, color, selectable);
        public ISwTempBody Add(ISwTempBody other) => m_MacroFeatureBodyContainer.Add(other);
        public ISwTempBody[] Subtract(ISwTempBody other) => m_MacroFeatureBodyContainer.Subtract(other);
        public ISwTempBody[] Common(ISwTempBody other) => m_MacroFeatureBodyContainer.Common(other);
        public void Transform(TransformMatrix transform) => m_MacroFeatureBodyContainer.Transform(transform);
        public void Dispose() => m_MacroFeatureBodyContainer.Dispose();
    }

    internal class SwSolidMacroFeatureEditBody : SwSolidBody, ISwMacroFeatureEditBody, ISwTempSolidBody
    {
        IXMemoryBody IXMemoryBody.Add(IXMemoryBody other) => Add((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Subtract(IXMemoryBody other) => Subtract((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Common(IXMemoryBody other) => Common((ISwTempBody)other);

        public bool IsPreviewMode { get; }

        public ISwTempBody PreviewBody => m_MacroFeatureBodyContainer.PreviewBody.Value;

        private readonly SwMacroFeatureBodyContainer m_MacroFeatureBodyContainer;

        internal SwSolidMacroFeatureEditBody(IBody2 body, SwDocument doc, SwApplication app, bool isPreview) : base(body, doc, app)
        {
            IsPreviewMode = isPreview;
            m_MacroFeatureBodyContainer = new SwMacroFeatureBodyContainer(body, doc, app, isPreview);
        }

        public void Preview(IXObject context, Color color, bool selectable) => m_MacroFeatureBodyContainer.Preview(context, color, selectable);
        public ISwTempBody Add(ISwTempBody other) => m_MacroFeatureBodyContainer.Add(other);
        public ISwTempBody[] Subtract(ISwTempBody other) => m_MacroFeatureBodyContainer.Subtract(other);
        public ISwTempBody[] Common(ISwTempBody other) => m_MacroFeatureBodyContainer.Common(other);
        public void Transform(TransformMatrix transform) => m_MacroFeatureBodyContainer.Transform(transform);
        public void Dispose() => m_MacroFeatureBodyContainer.Dispose();
    }

    internal class SwWireMacroFeatureEditBody : SwWireBody, ISwMacroFeatureEditBody, ISwTempWireBody
    {
        IXMemoryBody IXMemoryBody.Add(IXMemoryBody other) => Add((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Subtract(IXMemoryBody other) => Subtract((ISwTempBody)other);
        IXMemoryBody[] IXMemoryBody.Common(IXMemoryBody other) => Common((ISwTempBody)other);

        public bool IsPreviewMode { get; }

        public ISwTempBody PreviewBody => m_MacroFeatureBodyContainer.PreviewBody.Value;

        private readonly SwMacroFeatureBodyContainer m_MacroFeatureBodyContainer;

        internal SwWireMacroFeatureEditBody(IBody2 body, SwDocument doc, SwApplication app, bool isPreview) : base(body, doc, app)
        {
            IsPreviewMode = isPreview;
            m_MacroFeatureBodyContainer = new SwMacroFeatureBodyContainer(body, doc, app, isPreview);
        }

        public void Preview(IXObject context, Color color, bool selectable) => m_MacroFeatureBodyContainer.Preview(context, color, selectable);
        public ISwTempBody Add(ISwTempBody other) => m_MacroFeatureBodyContainer.Add(other);
        public ISwTempBody[] Subtract(ISwTempBody other) => m_MacroFeatureBodyContainer.Subtract(other);
        public ISwTempBody[] Common(ISwTempBody other) => m_MacroFeatureBodyContainer.Common(other);
        public void Transform(TransformMatrix transform) => m_MacroFeatureBodyContainer.Transform(transform);
        public void Dispose() => m_MacroFeatureBodyContainer.Dispose();
    }
}

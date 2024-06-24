//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Documents;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Utils.CustomFeature;

namespace Xarial.XCad.SolidWorks.Features.CustomFeature
{
    internal class SwMacroFeatureEditor<TData, TPage> : BaseCustomFeatureEditor<TData, TPage>
        where TData : class
        where TPage : class
    {
        internal event Func<IXDocument, IXCustomFeature<TData>, ISwObject> ProvidePreviewContext;

        internal SwMacroFeatureEditor(ISwApplication app, Type defType,
            IServiceProvider svcProvider,
            SwPropertyManagerPage<TPage> page, CustomFeatureEditorBehavior_e behavior) 
            : base(app, defType, svcProvider, page, behavior)
        {
        }
        
        protected override IXObject CurrentPreviewContext => ProvidePreviewContext?.Invoke(CurrentDocument, CurrentFeature);

        protected override void HidePreviewBody(IXMemoryBody body)
        {
            if (body is ISwMacroFeatureEditBody)
            {
                base.HidePreviewBody(((ISwMacroFeatureEditBody)body).PreviewBody);
            }
            else 
            {
                base.HidePreviewBody(body);
            }
        }

        protected override void CompleteFeature(PageCloseReasons_e reason)
        {
            base.CompleteFeature(reason);

            if (reason == PageCloseReasons_e.Okay || reason == PageCloseReasons_e.Apply) 
            {
                if (CurrentFeature.IsCommitted)
                {
                    var curMacroFeat = (SwMacroFeature<TData>)CurrentFeature;

                    if (curMacroFeat.UseCachedParameters)
                    {
                        curMacroFeat.ApplyParametersCache();
                        curMacroFeat.UseCachedParameters = false;
                    }
                }
            }
        }
    }
}
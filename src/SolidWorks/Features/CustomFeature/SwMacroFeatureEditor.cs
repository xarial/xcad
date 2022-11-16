//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swconst;
using System;
using System.Drawing;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Extensions;
using Xarial.XCad.Features.CustomFeature.Delegates;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.Toolkit.Utils;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Delegates;
using Xarial.XCad.Utils.CustomFeature;
using Xarial.XCad.Utils.Diagnostics;

namespace Xarial.XCad.SolidWorks.Features.CustomFeature
{
    internal class SwMacroFeatureEditor<TData, TPage> : BaseCustomFeatureEditor<TData, TPage>
        where TData : class
        where TPage : class
    {
        internal event Func<IXDocument, ISwObject> ProvidePreviewContext;

        private enum DisplayBodyResult_e 
        {
            Success = 0,
            NotTempBody = 1,
            InvalidComponent = 2,
            NotPart = 3
        }

        internal SwMacroFeatureEditor(ISwApplication app, Type defType,
            CustomFeatureParametersParser paramsParser, IServiceProvider svcProvider,
            SwPropertyManagerPage<TPage> page, CustomFeatureEditorBehavior_e behavior) 
            : base(app, defType, paramsParser, svcProvider, page, behavior)
        {
        }

        protected override void DisplayPreview(IXBody[] bodies, AssignPreviewBodyColorDelegate assignPreviewBodyColorDelegateFunc)
        {
            foreach (var body in bodies)
            {
                var swBody = (body as SwBody).Body;
                var previewContext = ProvidePreviewContext?.Invoke(CurModel)?.Dispatch;

                if (previewContext == null) 
                {
                    throw new Exception("Preview context is not specified");
                }

                assignPreviewBodyColorDelegateFunc.Invoke(body, out Color color);

                var res = (DisplayBodyResult_e)swBody.Display3(previewContext, ColorUtils.ToColorRef(color),
                    (int)swTempBodySelectOptions_e.swTempBodySelectOptionNone);

                if (res != DisplayBodyResult_e.Success) 
                {
                    throw new Exception($"Failed to render preview body: {res}");
                }

                var hasAlpha = color.A < 255;
                
                if (hasAlpha) 
                {
                    //COLORREF does not encode alpha channel, so assigning the color via material properties
                    body.Color = color;
                }
            }
        }

        protected override void HidePreview(IXBody[] bodies)
        {
            if (bodies != null)
            {
                for (int i = 0; i < bodies.Length; i++)
                {
                    if (bodies[i] is IDisposable)
                    {
                        try
                        {
                            (bodies[i] as IDisposable).Dispose();
                        }
                        catch (Exception ex)
                        {
                            m_Logger.Log(ex);
                        }
                    }

                    bodies[i] = null;
                }
            }
        }
    }
}
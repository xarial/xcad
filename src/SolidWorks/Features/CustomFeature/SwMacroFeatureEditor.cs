//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swconst;
using System;
using System.Drawing;
using Xarial.XCad.Base;
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
        where TData : class, new()
        where TPage : class, new()
    {
        private readonly Type m_HandlerType;

        internal SwMacroFeatureEditor(ISwApplication app, Type defType, Type handlerType,
            CustomFeatureParametersParser paramsParser, IServiceProvider svcProvider,
            CreateDynamicControlsDelegate createDynCtrlHandler) 
            : base(app, defType, paramsParser, svcProvider, createDynCtrlHandler)
        {
            m_HandlerType = handlerType;
        }

        protected override void DisplayPreview(IXBody[] bodies)
        {
            foreach (var body in bodies)
            {
                var swBody = (body as SwBody).Body;
                var model = (CurModel as SwDocument).Model;

                swBody.Display3(model, ColorUtils.ToColorRef(Color.Yellow),
                    (int)swTempBodySelectOptions_e.swTempBodySelectOptionNone);
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

        protected override IXPropertyPage<TPage> CreatePage(CreateDynamicControlsDelegate createDynCtrlHandler)
        {
            //TODO: add support for other options
            return new SwPropertyManagerPage<TPage>((ISwApplication)m_App, m_SvcProvider, m_HandlerType, createDynCtrlHandler);
        }
    }
}
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
        //private readonly AssignPreviewBodyColorDelegate m_AssignBodyColorFunc;

        internal SwMacroFeatureEditor(ISwApplication app, Type defType,
            CustomFeatureParametersParser paramsParser, IServiceProvider svcProvider,
            SwPropertyManagerPage<TPage> page) 
            : base(app, defType, paramsParser, svcProvider, page)
        {
            //m_AssignBodyColorFunc = assignPreviewBodyColorDelegateFunc;
        }

        protected override void DisplayPreview(IXBody[] bodies, AssignPreviewBodyColorDelegate assignPreviewBodyColorDelegateFunc)
        {
            foreach (var body in bodies)
            {
                var swBody = (body as SwBody).Body;
                var model = (CurModel as SwDocument).Model;

                assignPreviewBodyColorDelegateFunc.Invoke(body, out Color color);

                swBody.Display3(model, ColorUtils.ToColorRef(color),
                    (int)swTempBodySelectOptions_e.swTempBodySelectOptionNone);

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
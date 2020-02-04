//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swconst;
using System;
using System.Drawing;
using Xarial.XCad.Extensions;
using Xarial.XCad.Features.CustomFeature.Delegates;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.Utils.CustomFeature;

namespace Xarial.XCad.SolidWorks.Features.CustomFeature
{
    public class SwMacroFeatureEditor<TData, TPage> : BaseCustomFeatureEditor<TData, TPage>
        where TData : class, new()
        where TPage : class, new()
    {
        internal SwMacroFeatureEditor(IXApplication app, IXExtension ext, Type defType, CustomFeatureParametersParser paramsParser,
            DataConverterDelegate<TPage, TData> pageToDataConv, DataConverterDelegate<TData, TPage> dataToPageConv,
            CreateGeometryDelegate<TData> geomCreator) : base(app, ext, defType, paramsParser, pageToDataConv, dataToPageConv, geomCreator)
        {
        }

        protected override void DisplayPreview(IXBody[] bodies)
        {
            foreach (var body in bodies)
            {
                var swBody = (body as SwBody).Body;
                var model = (CurModel as SwDocument).Model;

                swBody.Display3(model, ConvertColor(Color.Yellow),
                    (int)swTempBodySelectOptions_e.swTempBodySelectOptionNone);
            }
        }

        private int ConvertColor(Color color)
        {
            return (color.R << 0) | (color.G << 8) | (color.B << 16);
        }

        protected override void HidePreview(IXBody[] bodies)
        {
            if (bodies != null)
            {
                for (int i = 0; i < bodies.Length; i++)
                {
                    if (bodies[i] is IDisposable)
                    {
                        (bodies[i] as IDisposable).Dispose();
                    }

                    bodies[i] = null;
                }
            }

            //TODO: check if this could be removed as it is causing flickering
            GC.Collect();
        }
    }
}
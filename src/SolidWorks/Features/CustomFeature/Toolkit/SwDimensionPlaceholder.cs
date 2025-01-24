//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using Xarial.XCad.SolidWorks.Annotations;

namespace Xarial.XCad.SolidWorks.Features.CustomFeature.Toolkit
{
    /// <summary>
    /// This is a mock implementation of display SOLIDWORKS dimension
    /// It is used in <see cref="XCad.Features.CustomFeature.Services.IParameterConverter.ConvertDisplayDimensions(XCad.Documents.IXDocument, XCad.Features.CustomFeature.IXCustomFeature, Xarial.XCad.Annotations.IXDimension[])"/>
    /// for supporting the backward compatibility of macro feature parameters
    /// </summary>
    internal class SwDimensionPlaceholder : SwDimension
    {
        internal SwDimensionPlaceholder() : base(default(IDisplayDimension), null, null)
        {            
        }

        public override double Value
        {
            get => double.NaN;
            set => base.Value = value;
        }
    }
}
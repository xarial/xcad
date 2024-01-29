//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Features.CustomFeature.Structures
{
    /// <summary>
    /// Information of the selection of <see cref="IXCustomFeature"/>
    /// </summary>
    public class CustomFeatureSelectionInfo 
    {
        /// <summary>
        /// Selection
        /// </summary>
        public IXSelObject Selection { get; }
        
        /// <summary>
        /// Transformation of this selection
        /// </summary>
        public TransformMatrix Transformation { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="selection">Selection</param>
        /// <param name="transformation">Transformation of the selection</param>
        public CustomFeatureSelectionInfo(IXSelObject selection, TransformMatrix transformation)
        {
            Selection = selection;
            Transformation = transformation;
        }
    }
}
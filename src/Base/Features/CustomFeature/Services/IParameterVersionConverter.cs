//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.XCad.Annotations;
using Xarial.XCad.Documents;
using Xarial.XCad.Features.CustomFeature.Structures;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Features.CustomFeature.Services
{
    /// <summary>
    /// Mechanism allowing to support backward compatibility of the macro feature parameters across the versions.
    /// This interface is coupled with <see cref="Attributes.ParametersVersionAttribute"/>
    /// </summary>
    /// <remarks>This class is a dictionary of version and the appropriate converter from the previous version to
    /// the specified one. Use <see cref="ParametersVersionConverter"/> for the specific implementation</remarks>
    public interface IParametersVersionConverter : IDictionary<Version, IParameterConverter>
    {
    }

    /// <summary>
    /// Specific implementation of <see cref="IParametersVersionConverter"/>
    /// </summary>
    public class ParametersVersionConverter : Dictionary<Version, IParameterConverter>, IParametersVersionConverter
    {
        public ParametersVersionConverter() : base()
        {
        }

        public ParametersVersionConverter(IDictionary<Version, IParameterConverter> dictionary)
            : base(dictionary)
        {
        }
    }

    /// <summary>
    /// Represents the conversion routines between this version of parameters and previous version of the parameters
    /// </summary>
    public interface IParameterConverter
    {
        /// <summary>
        /// Converts parameters from previous version
        /// </summary>
        /// <param name="model">Pointer to current model</param>
        /// <param name="feat">Pointer to current feature</param>
        /// <param name="parameters">Dictionary of parameter names and values</param>
        /// <param name="selection">Array of selections from previous version</param>
        /// <param name="dispDims">Array of display dimensions from the previous version</param>
        /// <param name="editBodies">Array of edit bodies form the previous version of parameters</param>
        /// <returns>Parameters for the new version</returns>
        /// <remarks>Parameters list also contains the indices for the objects in macro feature (edit bodies, selection, dimensions)</remarks>
        void Convert(IXDocument model, IXCustomFeature feat, ref Dictionary<string, object> parameters,
            ref CustomFeatureSelectionInfo[] selection, ref IXDimension[] dispDims, ref IXBody[] editBodies);
    }
}
//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.XCad.Annotations;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;

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
    /// <remarks>Use specific implementation <see cref="ParameterConverter"/></remarks>
    public interface IParameterConverter
    {
        /// <summary>
        /// Converts edit bodies from previous version
        /// </summary>
        /// <param name="model">Pointer to current model</param>
        /// <param name="feat">Pointer to current feature</param>
        /// <param name="editBodies">Array of edit bodies form the previous version of parameters</param>
        /// <returns>Array of new bodies in the new version of macro feature</returns>
        IXBody[] ConvertEditBodies(IXDocument model, IXCustomFeature feat, IXBody[] editBodies);

        /// <summary>
        /// Converts display dimensions from previous version
        /// </summary>
        /// <param name="model">Pointer to current model</param>
        /// <param name="feat">Pointer to current feature</param>
        /// <param name="dispDims">Array of display dimensions from the previous version</param>
        /// <returns>Array of display dimensions in the new version of macro feature</returns>
        IXDimension[] ConvertDisplayDimensions(IXDocument model, IXCustomFeature feat, IXDimension[] dispDims);

        /// <summary>
        /// Converts parameters from previous version
        /// </summary>
        /// <param name="model">Pointer to current model</param>
        /// <param name="feat">Pointer to current feature</param>
        /// <param name="parameters">Dictionary of parameter names and values</param>
        /// <returns>Parameters for the new version</returns>
        /// <remarks>Parameters list also contains the indices for the objects in macro feature (edit bodies, selection, dimensions)</remarks>
        Dictionary<string, string> ConvertParameters(IXDocument model, IXCustomFeature feat, Dictionary<string, string> parameters);

        /// <summary>
        /// Converts selections from previous version
        /// </summary>
        /// <param name="model">Pointer to current model</param>
        /// <param name="feat">Pointer to current feature</param>
        /// <param name="selection">Array of selections from previous version</param>
        /// <returns>Selections correspond to new version of macro feature</returns>
        IXSelObject[] ConvertSelections(IXDocument model, IXCustomFeature feat, IXSelObject[] selection);
    }

    /// <inheritdoc/>
    /// <summary>
    /// Specific implementation of <see cref="IParameterConverter"/>
    /// </summary>
    public class ParameterConverter : IParameterConverter
    {
        public virtual IXDimension[] ConvertDisplayDimensions(IXDocument model, IXCustomFeature feat, IXDimension[] dispDims)
        {
            return dispDims;
        }

        public virtual IXBody[] ConvertEditBodies(IXDocument model, IXCustomFeature feat, IXBody[] editBodies)
        {
            return editBodies;
        }

        public virtual Dictionary<string, string> ConvertParameters(IXDocument model, IXCustomFeature feat, Dictionary<string, string> parameters)
        {
            return parameters;
        }

        public virtual IXSelObject[] ConvertSelections(IXDocument model, IXCustomFeature feat, IXSelObject[] selection)
        {
            return selection;
        }
    }
}
//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Documents;

namespace Xarial.XCad.Features.CustomFeature
{
     /// <summary>
     /// Instance of the custom feature
     /// </summary>
    public interface IXCustomFeature : IXFeature
    {
        /// <summary>
        /// Type of the definition of this custom feature
        /// </summary>
        Type DefinitionType { get; set; }

        /// <summary>
        /// Referenced configuration
        /// </summary>
        IXConfiguration Configuration { get; }
    }

    /// <summary>
    /// Instance of the custom feature with parameters
    /// </summary>
    /// <typeparam name="TParams">Parameters data model</typeparam>
    public interface IXCustomFeature<TParams> : IXCustomFeature
        where TParams : class, new()
    {
        /// <summary>
        /// Parameters of this feature
        /// </summary>
        TParams Parameters { get; set; }
    }
}
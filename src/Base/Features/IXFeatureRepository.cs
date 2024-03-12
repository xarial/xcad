//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Base;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Features.Delegates;

namespace Xarial.XCad.Features
{
    /// <summary>
    /// Represents collection of features in the document
    /// </summary>
    public interface IXFeatureRepository : IXRepository<IXFeature>
    {
        /// <summary>
        /// Raised when new feature is created
        /// </summary>
        event FeatureCreatedDelegate FeatureCreated;

        /// <summary>
        /// Starts a custom feature insertion process with built-in editor for the property page
        /// </summary>
        ///<param name="featDefType">Definition of the custom feature</param>
        ///<param name="data">Feature data</param>
        void InsertCustomFeature(Type featDefType, object data);

        /// <summary>
        /// Enables or disables feature tree
        /// </summary>
        /// <param name="enable">True to enable, False to disable</param>
        void Enable(bool enable);
    }
}
//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Annotations;
using Xarial.XCad.Base;

namespace Xarial.XCad.Features
{
    /// <summary>
    /// Represents all features in the Feature Manager Design Tree
    /// </summary>
    public interface IXFeature : IXSelObject, IXColorizable, IXTransaction
    {
        /// <summary>
        /// Name of the feature
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Dimensions associated with the feature
        /// </summary>
        IXDimensionRepository Dimensions { get; }
    }
}
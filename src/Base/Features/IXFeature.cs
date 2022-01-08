//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Features
{
    /// <summary>
    /// Represents all features in the Feature Manager Design Tree
    /// </summary>
    public interface IXFeature : IXSelObject, IXColorizable, IDimensionable, IXTransaction
    {
        /// <summary>
        /// Name of the feature
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Faces of this feature
        /// </summary>
        IEnumerable<IXFace> Faces { get; }
    }
}
//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Features
{
    /// <summary>
    /// Specifies the state of the feature
    /// </summary>
    [Flags]
    public enum FeatureState_e 
    {
        /// <summary>
        /// Default state
        /// </summary>
        Default = 0,

        /// <summary>
        /// Feature is suppressed
        /// </summary>
        Suppressed = 1
    }

    /// <summary>
    /// Represents all features in the Feature Manager Design Tree
    /// </summary>
    public interface IXFeature : IXSelObject, IXColorizable, IDimensionable, IXTransaction, INameable
    {
        /// <summary>
        /// Faces of this feature
        /// </summary>
        IEnumerable<IXFace> Faces { get; }

        /// <summary>
        /// Identifies if this feature is standard (soldered) or a user created
        /// </summary>
        bool IsUserFeature { get; }

        /// <summary>
        /// State of this feature in the feature tree
        /// </summary>
        FeatureState_e State { get; }

        /// <summary>
        /// Component of this feature if it is a in-context feature of the assembly
        /// </summary>
        /// <remarks>Returns null if feature is top level feature</remarks>
        IXComponent Component { get; }

        /// <summary>
        /// Enables feature editing mode
        /// </summary>
        /// <returns>Feature edtior</returns>
        IEditor<IXFeature> Edit();
    }
}
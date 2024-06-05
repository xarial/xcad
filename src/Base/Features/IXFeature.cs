﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
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
    public interface IXFeature : IXSelObject, IXEntity, IHasColor, IDimensionable, IXTransaction, IHasName
    {
        /// <summary>
        /// Identifies if this feature is standard (soldered) or a user created
        /// </summary>
        bool IsUserFeature { get; }

        /// <summary>
        /// State of this feature in the feature tree
        /// </summary>
        FeatureState_e State { get; set; }

        /// <summary>
        /// Enables feature editing mode
        /// </summary>
        /// <returns>Feature edtior</returns>
        IEditor<IXFeature> Edit();
    }

    /// <summary>
    /// Additional method of the <see cref="IXFeature"/>
    /// </summary>
    public static class XFeatureExtension 
    {
        /// <summary>
        /// Iterates all bodies produced by this feature
        /// </summary>
        /// <param name="feat">Feature to iterate bodies</param>
        /// <returns>Bodies of the feture</returns>
        public static IEnumerable<IXBody> IterateBodies(this IXFeature feat)
        {
            var processedBodies = new List<IXBody>();

            foreach (var face in feat.AdjacentEntities.Filter<IXFace>())
            {
                var body = face.Body;

                if (!processedBodies.Any(b => b.Equals(body)))
                {
                    processedBodies.Add(body);
                    yield return body;
                }
            }
        }
    }
}
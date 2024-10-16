//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;

namespace Xarial.XCad.SolidWorks.Features.Extensions
{
    internal static class FeatureExtension 
    {
        internal static IEnumerable<IFeature> IterateFeatures(this IFeature firstFeature, bool recursive)
        {
            var processedFeats = new List<IFeature>();

            var nextFeat = firstFeature;

            while (nextFeat != null)
            {
                if (!processedFeats.Contains(nextFeat))
                {
                    processedFeats.Add(nextFeat);

                    yield return nextFeat;

                    if (recursive && nextFeat.GetTypeName2() != "HistoryFolder")
                    {
                        foreach (var subFeat in IterateSubFeatures(nextFeat, processedFeats, recursive))
                        {
                            yield return subFeat;
                        }
                    }
                }

                nextFeat = nextFeat.IGetNextFeature();
            }
        }

        internal static IEnumerable<IFeature> IterateSubFeatures(this IFeature parent, bool recursive)
            => IterateSubFeatures(parent, new List<IFeature>(), recursive);

        internal static IEnumerable<IDisplayDimension> IterateDisplayDimensions(this IFeature feat)
        {
            var featName = feat.Name;

            var dispDim = (IDisplayDimension)feat.GetFirstDisplayDimension();

            while (dispDim != null)
            {
                //NOTE: parent feature, such as extrude will also return all dimensions from child features, such as sketch
                var dimFeatName = dispDim.GetDimension2(0).FullName.Split('@')[1];

                if (string.Equals(dimFeatName, featName, StringComparison.CurrentCultureIgnoreCase))
                {
                    yield return dispDim;
                }

                dispDim = (IDisplayDimension)feat.GetNextDisplayDimension(dispDim);
            }
        }

        private static IEnumerable<IFeature> IterateSubFeatures(IFeature parent, List<IFeature> processedFeats, bool recursive)
        {
            var nextSubFeat = parent.IGetFirstSubFeature();

            while (nextSubFeat != null)
            {
                if (!processedFeats.Contains(nextSubFeat))
                {
                    processedFeats.Add(nextSubFeat);

                    yield return nextSubFeat;

                    if (recursive)
                    {
                        foreach (var subSubFeat in IterateSubFeatures(nextSubFeat, processedFeats, recursive))
                        {
                            yield return subSubFeat;
                        }
                    }
                }

                nextSubFeat = nextSubFeat.IGetNextSubFeature();
            }
        }
    }
}
//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System.Collections;
using System.Collections.Generic;
using Xarial.XCad.Features;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Features
{
    internal abstract class FeatureEnumerator : IEnumerator<IXFeature>
    {
        internal static IEnumerable<IFeature> IterateFeatures(IFeature firstFeature, bool recursive)
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

        internal static IEnumerable<IFeature> IterateSubFeatures(IFeature parent, bool recursive)
            => IterateSubFeatures(parent, new List<IFeature>(), recursive);

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

        public IXFeature Current
        {
            get
            {
                var feat = m_RootDoc.CreateObjectFromDispatch<SwFeature>(m_Features.Current);
                feat.SetContext(m_Context);
                return feat;
            }
        }

        object IEnumerator.Current => Current;

        private readonly ISwDocument m_RootDoc;

        private readonly IFeature m_FirstFeat;

        private readonly Context m_Context;

        internal FeatureEnumerator(ISwDocument rootDoc, IFeature firstFeat, Context context)
        {
            m_RootDoc = rootDoc;
            m_FirstFeat = firstFeat;
            m_Context = context;
        }

        public bool MoveNext() => m_Features.MoveNext();

        public void Reset()
            => m_Features = IterateFeatures(m_FirstFeat, true).GetEnumerator();

        private IEnumerator<IFeature> m_Features;

        public void Dispose()
        {
        }
    }
}
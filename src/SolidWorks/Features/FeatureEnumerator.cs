//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System.Collections;
using System.Collections.Generic;
using Xarial.XCad.Features;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Features
{
    internal abstract class FeatureEnumerator : IEnumerator<IXFeature>
    {
        public IXFeature Current => m_RootDoc.CreateObjectFromDispatch<SwFeature>(m_Features.Current);

        object IEnumerator.Current => Current;

        private readonly ISwDocument m_RootDoc;

        internal FeatureEnumerator(ISwDocument rootDoc)
        {
            m_RootDoc = rootDoc;
        }

        public bool MoveNext() => m_Features.MoveNext();

        public void Reset()
            => m_Features = IterateFeatures(GetFirstFeature()).GetEnumerator();

        private IEnumerator<IFeature> m_Features;

        protected abstract IFeature GetFirstFeature();

        private IEnumerable<IFeature> IterateFeatures(IFeature firstFeature)
        {
            var processedFeats = new List<IFeature>();

            IEnumerable<IFeature> IterateChildrenFeatures(IFeature parent)
            {
                var nextSubFeat = parent.IGetFirstSubFeature();

                while (nextSubFeat != null)
                {
                    if (!processedFeats.Contains(nextSubFeat))
                    {
                        processedFeats.Add(nextSubFeat);

                        yield return nextSubFeat;

                        foreach (var subSubFeat in IterateChildrenFeatures(nextSubFeat))
                        {
                            yield return subSubFeat;
                        }
                    }

                    nextSubFeat = nextSubFeat.IGetNextSubFeature();
                }
            }

            var nextFeat = firstFeature;

            while (nextFeat != null)
            {
                if (nextFeat.GetTypeName2() != "HistoryFolder")
                {
                    if (!processedFeats.Contains(nextFeat))
                    {
                        processedFeats.Add(nextFeat);

                        yield return nextFeat;

                        foreach (var subFeat in IterateChildrenFeatures(nextFeat))
                        {
                            yield return subFeat;
                        }
                    }
                }

                nextFeat = nextFeat.IGetNextFeature();
            }
        }

        public void Dispose()
        {
        }
    }
}
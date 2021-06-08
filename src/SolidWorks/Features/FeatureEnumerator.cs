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
        public IXFeature Current => SwObject.FromDispatch<SwFeature>(m_CurFeat, m_RootDoc);

        object IEnumerator.Current => Current;

        private IFeature m_CurFeat;

        private readonly List<IFeature> m_ProcessedFeatures;

        private bool m_IsSubFeat;
        private IFeature m_ParentFeat;

        private readonly ISwDocument m_RootDoc;

        internal FeatureEnumerator(ISwDocument rootDoc)
        {
            m_ProcessedFeatures = new List<IFeature>();
            m_RootDoc = rootDoc;
        }

        public void Dispose()
        {
        }

        private bool AddProcessedFeature()
        {
            if (!m_ProcessedFeatures.Contains(m_CurFeat))
            {
                m_ProcessedFeatures.Add(m_CurFeat);
                return true;
            }
            else
            {
                return MoveNext();
            }
        }

        public bool MoveNext()
        {
            if (m_IsSubFeat)
            {
                var subFeat = m_CurFeat.IGetNextSubFeature();

                if (subFeat != null)
                {
                    m_CurFeat = subFeat;
                    return AddProcessedFeature();
                }
                else
                {
                    m_IsSubFeat = false;
                    m_CurFeat = m_ParentFeat;
                }
            }
            else 
            {
                var subFeat = m_CurFeat.IGetFirstSubFeature();

                if (subFeat != null) 
                {
                    m_ParentFeat = m_CurFeat;
                    m_IsSubFeat = true;
                    m_CurFeat = subFeat;
                    return AddProcessedFeature();
                }
            }

            m_CurFeat = m_CurFeat.IGetNextFeature();

            if (m_CurFeat != null)
            {
                if (m_CurFeat.GetTypeName2() != "HistoryFolder")
                {
                    return AddProcessedFeature();
                }
                else 
                {
                    return MoveNext();
                }
            }
            else 
            {
                return false;
            }
        }

        public void Reset()
        {
            m_CurFeat = GetFirstFeature();
            m_ProcessedFeatures.Clear();
            m_ProcessedFeatures.Add(m_CurFeat);
            m_IsSubFeat = false;
        }

        protected abstract IFeature GetFirstFeature();
    }
}
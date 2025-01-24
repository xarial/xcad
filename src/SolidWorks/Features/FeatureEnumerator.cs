//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System.Collections;
using System.Collections.Generic;
using Xarial.XCad.Features;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.Extensions;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Features
{
    internal abstract class FeatureEnumerator : IEnumerator<IXFeature>
    {
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
            => m_Features = m_FirstFeat.IterateFeatures(true).GetEnumerator();

        private IEnumerator<IFeature> m_Features;

        public void Dispose()
        {
        }
    }
}
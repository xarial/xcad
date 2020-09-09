//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Features;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;

namespace Xarial.XCad.SolidWorks.Annotations
{
    public abstract class SwDimensionsCollection : IXDimensionRepository, IDisposable
    {
        IXDimension IXRepository<IXDimension>.this[string name] => this[name];

        public SwDimension this[string name]=> (SwDimension)this.Get(name);

        public abstract bool TryGet(string name, out IXDimension ent);

        public int Count => throw new NotImplementedException();

        public void AddRange(IEnumerable<IXDimension> ents)
        {
            throw new NotImplementedException();
        }

        public abstract IEnumerator<IXDimension> GetEnumerator();

        public void RemoveRange(IEnumerable<IXDimension> ents)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose()
        {
        }
    }

    internal class SwDocumentDimensionsCollection : SwDimensionsCollection
    {
        private readonly SwDocument m_Doc;

        internal SwDocumentDimensionsCollection(SwDocument model)
        {
            m_Doc = model;
        }

        public override IEnumerator<IXDimension> GetEnumerator() 
            => m_Doc.Features.SelectMany(f => f.Dimensions).GetEnumerator();

        public override bool TryGet(string name, out IXDimension ent)
        {
            var dimNameParts = name.Split('@');

            if (dimNameParts.Length != 2)
            {
                throw new Exception("Invalid dimension name. Name must be specified in the following format: DimName@FeatureName");
            }

            var dimName = dimNameParts[0];
            var featName = dimNameParts[1];

            IXDimension dim = null;

            if (m_Doc.Features.TryGet(featName, out IXFeature feat))
            {
                dim = feat.Dimensions.FirstOrDefault(
                    d => string.Equals(d.Name, dimName, StringComparison.CurrentCultureIgnoreCase));
            }

            if (dim != null)
            {
                ent = dim;
                return true;
            }
            else
            {
                ent = null;
                return false;
            }
        }
    }

    internal class SwFeatureDimensionsCollection : SwDimensionsCollection
    {
        private readonly SwFeature m_Feat;

        private readonly SwDocument m_Doc;

        internal SwFeatureDimensionsCollection(SwDocument doc, SwFeature feat)
        {
            m_Doc = doc;
            m_Feat = feat;
        }

        public override bool TryGet(string name, out IXDimension ent)
        {
            var dimNameParts = name.Split('@');

            var dimName = dimNameParts[0];
            var featName = "";

            if (dimNameParts.Length == 2)
            {
                featName = dimNameParts[0];

                if (!string.Equals(featName, m_Feat.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new Exception("Specified dimension does not belong to this feature");
                }
            }

            var dim = this.FirstOrDefault(
                d => string.Equals(d.Name, dimName,
                StringComparison.CurrentCultureIgnoreCase));

            if (dim != null)
            {
                ent = dim;
                return true;
            }
            else
            {
                ent = null;
                return false;
            }
        }

        public override IEnumerator<IXDimension> GetEnumerator() 
            => new SwFeatureDimensionsEnumerator(m_Doc, m_Feat.Feature);
    }

    internal class SwFeatureDimensionsEnumerator : IEnumerator<IXDimension>
    {
        public IXDimension Current => SwSelObject.FromDispatch<SwDimension>(m_CurDispDim, m_Doc);

        object IEnumerator.Current => Current;

        private readonly SwDocument m_Doc;

        private readonly IFeature m_Feat;

        private IDisplayDimension m_CurDispDim;

        private bool m_IsStart;

        internal SwFeatureDimensionsEnumerator(SwDocument doc, IFeature feat) 
        {
            m_Doc = doc;
            m_Feat = feat;
            m_IsStart = true;
        }        

        public bool MoveNext()
        {
            if (m_IsStart)
            {
                m_IsStart = false;
                m_CurDispDim = m_Feat.GetFirstDisplayDimension() as IDisplayDimension;
            }
            else 
            {
                m_CurDispDim = m_Feat.GetNextDisplayDimension(m_CurDispDim) as IDisplayDimension;
            }

            if (m_CurDispDim != null)
            {
                //NOTE: parent feature, such as extrude will also return all dimensions from child features, such as sketch
                var featName = m_CurDispDim.GetDimension2(0).FullName.Split('@')[1];

                if (!string.Equals(featName, m_Feat.Name, StringComparison.CurrentCultureIgnoreCase)) 
                {
                    return MoveNext();
                }
            }

            return m_CurDispDim != null;
        }

        public void Reset()
        {
            m_IsStart = true;
        }

        public void Dispose()
        {
        }
    }
}

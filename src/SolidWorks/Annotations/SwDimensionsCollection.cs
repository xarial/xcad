using SolidWorks.Interop.sldworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Annotations;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Annotations
{
    public class SwDimensionsCollection : IXDimensionsRepository
    {
        private readonly SwDocument m_Doc;

        IXDimension IXDimensionsRepository.this[string name] => this[name];

        public SwDimension this[string name] 
        {
            get 
            {
                var dimNameParts = name.Split('@');

                if (dimNameParts.Length != 2) 
                {
                    throw new Exception("Invalid dimension name. Name must be specified in the following format: DimName@FeatureName");
                }

                var dimName = dimNameParts[0];
                var featName = dimNameParts[1];

                var feat = m_Doc.Features[featName];

                var dim = feat.Dimensions.FirstOrDefault(
                    d => string.Equals(d.Dimension.Name, dimName, StringComparison.CurrentCultureIgnoreCase));

                if (dim != null)
                {
                    return dim;
                }
                else 
                {
                    throw new Exception($"Failed to find {dimName} in {featName}");
                }
            }
        }

        public int Count => throw new NotImplementedException();

        internal SwDimensionsCollection(SwDocument model) 
        {
            m_Doc = model;
        }

        public void AddRange(IEnumerable<IXDimension> ents)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IXDimension> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<IXDimension> ents)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}

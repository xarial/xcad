using Inventor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Features.Delegates;
using Xarial.XCad.Geometry;
using Xarial.XCad.Inventor.Documents;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.Inventor.Features
{
    /// <summary>
    /// Autodesk inventor specific features collection
    /// </summary>
    public interface IAiPartFeaturesCollection : IXFeatureRepository
    {
        /// <summary>
        /// Part features collection
        /// </summary>
        PartFeatures Features { get; }
    }

    internal class AiPartFeaturesCollection : IAiPartFeaturesCollection
    {
        public event FeatureCreatedDelegate FeatureCreated;

        private readonly AiPart m_Part;

        private readonly RepositoryHelper<IXFeature> m_RepoHelper;

        public AiPartFeaturesCollection(AiPart part)
        {
            m_Part = part;
            m_RepoHelper = new RepositoryHelper<IXFeature>(this,
                TransactionFactory<IXFeature>.Create(() => new AiFlatPatternFeature((SheetMetalComponentDefinition)m_Part.Part.ComponentDefinition, null, m_Part, m_Part.OwnerApplication)));
        }

        public IXFeature this[string name] => m_RepoHelper.Get(name);

        public int Count => Features.Count;

        public void AddRange(IEnumerable<IXFeature> ents, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Enable(bool enable)
        {
            throw new NotImplementedException();
        }

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters)
        {
            bool flatPatterns;
            bool all;

            if (filters?.Any() == true)
            {
                flatPatterns = false;
                all = false;

                foreach (var filter in filters)
                {
                    if (typeof(IXFlatPattern).IsAssignableFrom(filter.Type))
                    {
                        flatPatterns = true;
                    }
                    else if (filter.Type == null || typeof(IXFeature).IsAssignableFrom(filter.Type))
                    {
                        all = true;
                        break;
                    }
                }
            }
            else
            {
                flatPatterns = true;
                all = true;
            }

            foreach (var ent in m_RepoHelper.FilterDefault(IterateFeatures(flatPatterns, all), filters, reverseOrder))
            {
                yield return ent;
            }
        }

        private IEnumerable<IXFeature> IterateFeatures(bool flatPatterns, bool all)
        {
            if (all)
            {
                return IterateAllFeatures();
            }
            else if (flatPatterns)
            {
                return IterareFlatPatternFeatures();
            }
            else 
            {
                return Enumerable.Empty<IXFeature>();
            }
        }

        public IEnumerator<IXFeature> GetEnumerator() => IterateAllFeatures().GetEnumerator();

        private IEnumerable<AiFeature> IterateAllFeatures()
        {
            foreach (PartFeature feat in Features)
            {
                yield return m_Part.OwnerApplication.CreateObjectFromDispatch<AiFeature>(feat, m_Part);
            }

            foreach (var flatPatternFeat in IterareFlatPatternFeatures())
            {
                yield return flatPatternFeat;
            }
        }

        private IEnumerable<AiFlatPatternFeature> IterareFlatPatternFeatures()
        {
            if (m_Part.Part.ComponentDefinition is SheetMetalComponentDefinition)
            {
                var sheetMetalCompDef = (SheetMetalComponentDefinition)m_Part.Part.ComponentDefinition;

                if (sheetMetalCompDef.HasFlatPattern)
                {
                    yield return new AiFlatPatternFeature(sheetMetalCompDef, new FlatPatternFeature(), m_Part, m_Part.OwnerApplication);
                }
            }
        }

        public void InsertCustomFeature(Type featDefType, object data)
        {
            throw new NotImplementedException();
        }

        public T PreCreate<T>() where T : IXFeature
            => m_RepoHelper.PreCreate<T>();

        public void RemoveRange(IEnumerable<IXFeature> ents, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public bool TryGet(string name, out IXFeature ent)
        {
            var feat = Features[name];

            if (feat != null)
            {
                ent = m_Part.OwnerApplication.CreateObjectFromDispatch<AiFeature>(feat, m_Part);
                return true;
            }
            else 
            {
                ent = null;
                return false;

            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public PartFeatures Features => m_Part.Part.ComponentDefinition.Features;
    }
}

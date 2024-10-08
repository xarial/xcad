//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Features;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Annotations
{
    /// <summary>
    /// SOLIDWORKS-specific dimensions collection
    /// </summary>
    public interface ISwDimensionsCollection : IXDimensionRepository, IDisposable
    {
        /// <inheritdoc/>
        new ISwDimension this[string name] { get; }
    }

    internal class SwDimensionEntityCache : EntityCache<IXDimension>
    {
        private readonly SwDimensionsCollection m_DimsColl;

        public SwDimensionEntityCache(IXTransaction owner, SwDimensionsCollection dimsColl, Func<IXDimension, string> nameProvider) : base(owner, dimsColl, nameProvider)
        {
            m_DimsColl = dimsColl;
        }

        protected override void CommitEntitiesFromCache(IReadOnlyList<IXDimension> ents, CancellationToken cancellationToken)
        {
            var newDims = new List<SwDimension>();

            foreach (SwDimension cachedDim in ents)
            {
                if (cachedDim.IsCommitted)
                {
                    cachedDim.CommitCachedValue();
                }
                else
                {
                    newDims.Add(cachedDim);
                }
            }

            base.CommitEntitiesFromCache(newDims, cancellationToken);
        }

        protected override IEnumerable<IXDimension> IterateEntities(IReadOnlyList<IXDimension> ents)
        {
            foreach (var cachedEnt in ents)
            {
                yield return cachedEnt;
            }

            foreach (var dim in m_DimsColl.IterateActualDimensions()) 
            {
                if (!ents.Any(d => string.Equals(d.Name, dim.Name, StringComparison.CurrentCultureIgnoreCase)))
                {
                    AddRange(new IXDimension[] { dim }, default);
                    yield return dim;
                }
            }
        }

        public override bool TryGet(string name, out IXDimension ent)
        {
            if (base.TryGet(name, out ent))
            {
                return true;
            }
            else 
            {
                if (m_DimsColl.TryGetActualDimension(name, out var dim))
                {
                    //NOTE: adding the dimension to cache so its value can be committed if changed
                    AddRange(new IXDimension[] { dim }, default);
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
    }

    internal abstract class SwDimensionsCollection : ISwDimensionsCollection
    {
        IXDimension IXRepository<IXDimension>.this[string name] => this[name];
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public ISwDimension this[string name] => (ISwDimension)m_RepoHelper.Get(name);

        public abstract bool TryGet(string name, out IXDimension ent);

        public int Count => throw new NotImplementedException();

        protected readonly Context m_Context;

        private readonly RepositoryHelper<IXDimension> m_RepoHelper;

        protected readonly EntityCache<IXDimension> m_Cache;

        protected SwDimensionsCollection(Context context) 
        {
            m_Context = context;
            m_RepoHelper = new RepositoryHelper<IXDimension>(this);

            m_Cache = new SwDimensionEntityCache(context.Owner, this, d => d.Name);
        }

        public T PreCreate<T>() where T : IXDimension => throw new NotImplementedException();

        public void AddRange(IEnumerable<IXDimension> ents, CancellationToken cancellationToken)
        {
            if (m_Context.Owner.IsCommitted)
            {
                throw new NotImplementedException();
            }
            else 
            {
                m_Cache.AddRange(ents, cancellationToken);
            }
        }

        public abstract IEnumerator<IXDimension> GetEnumerator();

        internal abstract bool TryGetActualDimension(string name, out SwDimension dim);
        internal abstract IEnumerable<SwDimension> IterateActualDimensions();

        public void RemoveRange(IEnumerable<IXDimension> ents, CancellationToken cancellationToken)
        {
            if (m_Context.Owner.IsCommitted)
            {
                throw new NotImplementedException();
            }
            else
            {
                m_Cache.RemoveRange(ents, cancellationToken);
            }
        }

        internal void CommitCache(CancellationToken cancellationToken) 
            => m_Cache.Commit(cancellationToken);

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) 
            => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public void Dispose()
        {
        }
    }

    internal class SwFeatureManagerDimensionsCollection : SwDimensionsCollection
    {
        private readonly SwFeatureManager m_FeatMgr;

        internal SwFeatureManagerDimensionsCollection(SwFeatureManager featMgr, Context context) : base(context)
        {
            m_FeatMgr = featMgr;
        }

        public override IEnumerator<IXDimension> GetEnumerator()
        {
            if (m_Context.Owner.IsCommitted)
            {
                return IterateActualDimensions().GetEnumerator();
            }
            else 
            {
                return m_Cache.GetEnumerator();
            }
        }

        public override bool TryGet(string name, out IXDimension ent)
        {
            if (m_Context.Owner.IsCommitted)
            {
                var res = TryGetActualDimension(name, out var dim);
                ent = dim;
                return res;
            }
            else 
            {
                return m_Cache.TryGet(name, out ent);
            }
        }

        internal override IEnumerable<SwDimension> IterateActualDimensions()
            => m_FeatMgr.SelectMany(f =>
            {
                ((SwFeature)f).SetContext(m_Context);
                return ((SwFeatureDimensionsCollection)f.Dimensions).IterateActualDimensions();
            });

        internal override bool TryGetActualDimension(string name, out SwDimension dim)
        {
            var dimNameParts = name.Split('@');

            if (dimNameParts.Length != 2)
            {
                throw new Exception("Invalid dimension name. Name must be specified in the following format: DimName@FeatureName");
            }

            var dimName = dimNameParts[0];
            var featName = dimNameParts[1];

            if (m_FeatMgr.TryGet(featName, out IXFeature feat))
            {
                //NOTE: not using IModelDoc2::Parameter because it returns IDimension and theer is no conversion to IDisplayDimension
                return ((SwFeatureDimensionsCollection)feat.Dimensions).TryGetActualDimension($"{dimName}@{featName}", out dim);
            }
            else
            {
                dim = null;
                return false;
            }
        }
    }

    internal class SwFeatureDimensionsCollection : SwDimensionsCollection
    {
        private readonly ISwDocument m_Doc;
        private readonly SwFeature m_Feat;

        internal SwFeatureDimensionsCollection(SwFeature feat, ISwDocument doc, Context context) : base(context)
        {
            m_Feat = feat;
            m_Doc = doc;
        }

        public override bool TryGet(string name, out IXDimension ent)
        {
            if (m_Context.Owner.IsCommitted)
            {
                var res = TryGetActualDimension(name, out var swDim);
                ent = swDim;
                return res;
            }
            else 
            {
                return m_Cache.TryGet(name, out ent);
            }
        }

        public override IEnumerator<IXDimension> GetEnumerator()
        {
            if (m_Context.Owner.IsCommitted)
            {
                return IterateActualDimensions().GetEnumerator();
            }
            else 
            {
                return m_Cache.GetEnumerator();
            }
        }

        internal override bool TryGetActualDimension(string name, out SwDimension dim)
        {
            var dimNameParts = name.Split('@');

            var dimName = dimNameParts[0];
            var featName = "";

            if (dimNameParts.Length == 2)
            {
                featName = dimNameParts[1];

                if (!string.Equals(featName, m_Feat.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new Exception("Specified dimension does not belong to this feature");
                }
            }

            dim = IterateActualDimensions().FirstOrDefault(
                d => string.Equals(d.Name, $"{dimName}@{featName}",
                StringComparison.CurrentCultureIgnoreCase));

            if (dim != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal override IEnumerable<SwDimension> IterateActualDimensions()
            => IterateDisplayDimensions().Select(d =>
            {
                var dim = m_Doc.CreateObjectFromDispatch<SwDimension>(d);
                dim.SetContext(m_Context);
                return dim;
            });

        private IEnumerable<IDisplayDimension> IterateDisplayDimensions() 
        {
            var dispDim = (IDisplayDimension)m_Feat.Feature.GetFirstDisplayDimension();

            while (dispDim != null)
            {
                //NOTE: parent feature, such as extrude will also return all dimensions from child features, such as sketch
                var featName = dispDim.GetDimension2(0).FullName.Split('@')[1];

                if (string.Equals(featName, m_Feat.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    yield return dispDim;
                }

                dispDim = (IDisplayDimension)m_Feat.Feature.GetNextDisplayDimension(dispDim);
            }
        }
    }
}

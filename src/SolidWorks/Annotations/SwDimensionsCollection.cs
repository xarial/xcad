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
using Xarial.XCad.SolidWorks.Features.Extensions;
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

        public SwDimensionEntityCache(IXObject owner, SwDimensionsCollection dimsColl, Func<IXDimension, string> nameProvider) : base(owner, dimsColl, nameProvider)
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
            var dimParam = (IDimension)m_FeatMgr.Document.Model.Parameter(name);

            if (dimParam != null)
            {
                dim = m_FeatMgr.Document.CreateObjectFromDispatch<SwDimension>(dimParam);
                dim.SetContext(m_Context);
                return true;
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
            if (name.Contains("@")) 
            {
                var nameParts = name.Split('@');

                name = nameParts[0];

                if (!string.Equals(m_Feat.Name, nameParts[1], StringComparison.CurrentCultureIgnoreCase)) 
                {
                    throw new Exception("Dimension does not belong to the feature");
                }
            }

            var dimParam = (IDimension)m_Feat.Feature.Parameter(name);

            if (dimParam != null)
            {
                dim = m_Doc.CreateObjectFromDispatch<SwDimension>(dimParam);
                dim.SetContext(m_Context);
                return true;
            }
            else
            {
                dim = null;
                return false;
            }
        }

        internal override IEnumerable<SwDimension> IterateActualDimensions()
            => m_Feat.Feature.IterateDisplayDimensions().Select(d =>
            {
                var dim = m_Doc.CreateObjectFromDispatch<SwDimension>(d);
                dim.SetContext(m_Context);
                return dim;
            });
    }
}

using Inventor;
using System;
using System.Diagnostics;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Inventor.Documents;
using Xarial.XCad.Services;

namespace Xarial.XCad.Inventor.Features
{
    /// <summary>
    /// Autodesk Inventor specific feature
    /// </summary>
    public interface IAiFeature : IXFeature, IAiSelObject
    {
        /// <summary>
        /// Pointer to feature
        /// </summary>
        PartFeature Feature { get; }
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class AiFeature : AiSelObject, IAiFeature
    {
        internal static AiFeature New(PartFeature feat, AiDocument ownerDoc, AiApplication ownerApp) 
            => new AiFeature(feat, ownerDoc, ownerApp);

        private readonly ElementCreator<PartFeature> m_Creator;

        protected AiFeature(PartFeature feat, AiDocument ownerDoc, AiApplication ownerApp) : base(feat, ownerDoc, ownerApp)
        {
            m_Creator = new ElementCreator<PartFeature>(CreateFeature, feat, feat != null);
        }

        public override bool IsCommitted => m_Creator.IsCreated;

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        protected virtual PartFeature CreateFeature(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public PartFeature Feature => m_Creator.Element;

        public IXIdentifier Id => throw new NotImplementedException();

        public bool IsUserFeature => throw new NotImplementedException();

        public FeatureState_e State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IXComponent Component => throw new NotImplementedException();

        public IXBody Body => throw new NotImplementedException();

        public IXEntityRepository AdjacentEntities => throw new NotImplementedException();

        public System.Drawing.Color? Color { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IXDimensionRepository Dimensions => throw new NotImplementedException();

        public virtual string Name 
        {
            get => Feature.Name;
            set => Feature.Name = value; 
        }

        public IEditor<IXFeature> Edit()
        {
            throw new NotImplementedException();
        }

        public XCad.Geometry.Structures.Point FindClosestPoint(XCad.Geometry.Structures.Point point)
        {
            throw new NotImplementedException();
        }
    }
}

using Inventor;
using System;
using System.Threading;
using Xarial.XCad.Documents;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Inventor.Documents;

namespace Xarial.XCad.Inventor.Features
{
    internal class FlatPatternFeature : PartFeature, IAiArtificialEntity
    {
        public void Delete(bool RetainConsumedSketches = false, bool RetainDependentFeaturesAndSketches = false, bool RetainDependentWorkFeatures = false)
        {
            throw new NotImplementedException();
        }

        public RenderStyle GetRenderStyle(out StyleSourceTypeEnum StyleSourceType)
        {
            throw new NotImplementedException();
        }

        public void SetRenderStyle(StyleSourceTypeEnum StyleSourceType, object RenderStyle)
        {
            throw new NotImplementedException();
        }

        public void GetReferenceKey(ref byte[] ReferenceKey, int KeyContext = 0)
        {
            throw new NotImplementedException();
        }

        public void SetEndOfPart(bool Before)
        {
            throw new NotImplementedException();
        }

        public void RemoveParticipant(ComponentOccurrence Occurrence)
        {
            throw new NotImplementedException();
        }

        public bool GetSuppressionCondition(out Parameter Parameter, out ComparisonTypeEnum ComparisonType, out object Expression)
        {
            throw new NotImplementedException();
        }

        public void SetSuppressionCondition(Parameter Parameter, ComparisonTypeEnum ComparisonType, object Expression)
        {
            throw new NotImplementedException();
        }

        public void SetAffectedBodies(ObjectCollection Bodies)
        {
            throw new NotImplementedException();
        }

        public ObjectTypeEnum Type => throw new NotImplementedException();

        public object Application => throw new NotImplementedException();

        public ComponentDefinition Parent => throw new NotImplementedException();

        public AttributeSets AttributeSets => throw new NotImplementedException();

        public string Name { get => "Flat Pattern"; set => throw new NotImplementedException(); }

        public HealthStatusEnum HealthStatus => throw new NotImplementedException();

        public bool Adaptive { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Box RangeBox => throw new NotImplementedException();

        public Faces Faces => throw new NotImplementedException();

        public SurfaceBody SurfaceBody => throw new NotImplementedException();

        public bool Suppressed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ComponentOccurrencesEnumerator Participants => throw new NotImplementedException();

        public ParametersEnumerator Parameters => throw new NotImplementedException();

        public bool Shared { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool ConsumeInputs { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsOwnedByFeature => throw new NotImplementedException();

        public PartFeature OwnedBy => throw new NotImplementedException();

        public FeatureDimensions FeatureDimensions => throw new NotImplementedException();

        public SurfaceBodies SurfaceBodies => throw new NotImplementedException();

        public string ExtendedName => throw new NotImplementedException();

        public Asset Appearance { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public AppearanceSourceTypeEnum AppearanceSourceType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    /// <summary>
    /// Inventor-specific flat pattern
    /// </summary>
    public interface IAiFlatPattern : IXFlatPattern
    {
        /// <summary>
        /// Pointer to underlying flat pattern element
        /// </summary>
        FlatPattern FlatPattern { get; }
    }

    internal class AiFlatPatternFeature : AiFeature, IAiFlatPattern
    {
        private readonly SheetMetalComponentDefinition m_SheetMetalCompDef;

        public FlatPattern FlatPattern 
        {
            get 
            {
                if (IsCommitted)
                {
                    return m_SheetMetalCompDef.FlatPattern;
                }
                else 
                {
                    throw new NonCommittedElementAccessException();
                }
            }
        }

        internal AiFlatPatternFeature(SheetMetalComponentDefinition sheetMetalCompDef, FlatPatternFeature feat, AiDocument ownerDoc, AiApplication ownerApp) : base(feat, ownerDoc, ownerApp)
        {
            m_SheetMetalCompDef = sheetMetalCompDef;
        }

        public bool IsFlattened { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IXEntity FixedEntity => throw new NotImplementedException();

        public IFlatPatternSaveOperation PreCreateSaveAsOperation(string filePath)
            => new AiFlatPatternSaveOperation(OwnerDocument, m_SheetMetalCompDef, OwnerDocument.TryGetTranslator(filePath), filePath);

        protected override PartFeature CreateFeature(CancellationToken token)
        {
            if (!m_SheetMetalCompDef.HasFlatPattern)
            {
                m_SheetMetalCompDef.Unfold();
                return new FlatPatternFeature();
            }
            else 
            {
                throw new NotSupportedException("Flat pattern already created");
            }
        }
    }
}

//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Utils;
using System.Linq;
using Xarial.XCad.Toolkit;

namespace Xarial.XCad.SolidWorks.Features
{
    public interface ISwFeature : ISwSelObject, IXFeature, ISwEntity, ISupportsResilience<ISwFeature>
    {
        IFeature Feature { get; }
        new ISwDimensionsCollection Dimensions { get; }
        new ISwComponent Component { get; }
    }

    internal abstract class SwFeatureEditor<TFeatData> : IEditor<SwFeature>
    {
        public SwFeature Target { get; }

        public bool Cancel 
        {
            get;
            set;
        }

        private readonly TFeatData m_FeatData;
        private readonly ISwDocument m_Doc;
        private readonly ISwComponent m_Comp;

        internal SwFeatureEditor(SwFeature feat, TFeatData featData)
        {
            Target = feat;
            m_FeatData = featData;

            m_Doc = Target.OwnerDocument;
            m_Comp = Target.Component;

            if (!StartEdit(m_FeatData, m_Doc, m_Comp)) 
            {
                throw new Exception("Failed to start editing of the feature");
            }
        }

        protected abstract bool StartEdit(TFeatData featData, ISwDocument doc, ISwComponent comp);
        protected abstract void CancelEdit(TFeatData featData);

        private void EndEdit(bool cancel)
        {
            if (!cancel)
            {
                if (!Target.Feature.ModifyDefinition(m_FeatData, m_Doc.Model, m_Comp?.Component))
                {
                    throw new Exception("Failed to modify definition of the feature");
                }
            }
            else 
            {
                CancelEdit(m_FeatData);
            }
        }

        public void Dispose()
        {
            EndEdit(Cancel);
        }
    }

    internal class SwFeatureEntityRepository : SwEntityRepository
    {
        private readonly SwFeature m_Feat;

        internal SwFeatureEntityRepository(SwFeature feat) 
        {
            m_Feat = feat;
        }

        protected override IEnumerable<ISwEntity> IterateEntities(bool faces, bool edges, bool vertices, bool silhouetteEdges)
        {
            if (faces)
            {
                var featFaces = (object[])m_Feat.Feature.GetFaces();

                if (featFaces != null)
                {
                    foreach (var face in featFaces)
                    {
                        yield return m_Feat.OwnerDocument.CreateObjectFromDispatch<ISwFace>(face);
                    }
                }
            }
        }
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class SwFeature : SwSelObject, ISwFeature
    {
        private static string[] m_SolderedFeatureTypes = new string[]
        {
            "CommentsFolder",
            "FavoriteFolder",
            "HistoryFolder",
            "SelectionSetFolder",
            "SensorFolder",
            "DocsFolder",
            "DetailCabinet",
            "NotesAreaFtrFolder",
            "SurfaceBodyFolder",
            "SolidBodyFolder",
            "EnvFolder",
            "AmbientLight",
            "DirectionLight",
            "InkMarkupFolder",
            "EqnFolder",
            "MaterialFolder",
            SwOrigin.TypeName,
            "LiveSectionFolder",
            "MateGroup",
            "BlockFolder",
            "MarkupCommentFolder",
            "DrSheet",
            "DetailFolder",
            "DrTemplate",
            "GeneralTableAnchor",
            "BomTemplate",
            "HoleTableAnchor",
            "WeldmentTableAnchor",
            "RevisionTableAnchor",
            "WeldTableAnchor",
            "BendTableAnchor",
            "PunchTableAnchor",
            "EditBorderFeature"
        };

        IXBody IXEntity.Body => Body;
        IXEntityRepository IXEntity.AdjacentEntities => AdjacentEntities;
        ISwEntity ISupportsResilience<ISwEntity>.CreateResilient() => CreateResilient();
        IXComponent IXEntity.Component => Component;
        IXDimensionRepository IDimensionable.Dimensions => Dimensions;
        IXObject ISupportsResilience.CreateResilient() => CreateResilient();

        protected readonly IElementCreator<IFeature> m_Creator;

        public virtual IFeature Feature 
        {
            get
            {
                var feat = m_Creator.Element;

                if (IsResilient)
                {
                    try
                    {
                        var testPtrAlive = feat.Name;
                    }
                    catch
                    {
                        var restoredFeat = (IFeature)OwnerDocument.Model.Extension.GetObjectByPersistReference3(m_PersistId, out _);

                        if (restoredFeat != null)
                        {
                            feat = restoredFeat;
                            m_Creator.Set(feat);
                        }
                        else
                        {
                            throw new NullReferenceException("Pointer to the feature cannot be restored");
                        }
                    }
                }

                return feat;
            }
        }

        public IXIdentifier Id => new XIdentifier(Feature.GetID());

        public override object Dispatch => Feature;

        public override bool IsAlive => this.CheckIsAlive(() => Feature.GetID());

        public bool IsResilient { get; private set; }

        private byte[] m_PersistId;

        private readonly Lazy<SwFeatureDimensionsCollection> m_DimensionsLazy;
        private Context m_Context;

        internal SwFeature(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(feat, doc, app)
        {
            if (doc == null) 
            {
                throw new ArgumentNullException(nameof(doc));
            }

            m_DimensionsLazy = new Lazy<SwFeatureDimensionsCollection>(
                () => new SwFeatureDimensionsCollection(this, OwnerDocument, GetContext()));

            m_Creator = new ElementCreator<IFeature>(CreateFeature, CommitCache, feat, created);

            AdjacentEntities = new SwFeatureEntityRepository(this);
        }

        internal void SetContext(Context context) 
        {
            m_Context = context;
        }

        internal virtual SwFeature Clone(Context context)
        {
            var feat = OwnerDocument.CreateObjectFromDispatch<SwFeature>(Feature);
            feat.SetContext(context);

            return feat;
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        public virtual ISwFeature CreateResilient()
        {
            if (OwnerDocument == null)
            {
                throw new NullReferenceException("Owner document is not set");
            }

            var id = (byte[])OwnerDocument.Model.Extension.GetPersistReference3(Feature);

            if (id != null)
            {
                var feat = OwnerDocument.CreateObjectFromDispatch<SwFeature>(Feature);
                feat.MakeResilient(id);
                return feat;
            }
            else
            {
                throw new Exception("Failed to create resilient feature");
            }
        }

        private void MakeResilient(byte[] persistId)
        {
            IsResilient = true;
            m_PersistId = persistId;
        }

        public ISwComponent Component
        {
            get
            {
                var comp = (IComponent2)((IEntity)Feature).GetComponent();

                if (comp != null)
                {
                    return OwnerDocument.CreateObjectFromDispatch<ISwComponent>(comp);
                }
                else
                {
                    return null;
                }
            }
        }

        private IFeature CreateFeature(CancellationToken cancellationToken)
        {
            using (var viewFreeze = new UiFreeze(OwnerDocument))
            {
                var feat = InsertFeature(cancellationToken);

                if (feat != null)
                {
                    var userName = Name;

                    if (!string.IsNullOrEmpty(userName))
                    {
                        feat.Name = userName;
                    }

                    var userColor = Color;

                    if (userColor.HasValue)
                    {
                        SetColor(feat, userColor);
                    }

                    return feat;
                }
                else 
                {
                    throw new Exception("Failed to insert feature");
                }
            }
        }

        protected virtual IFeature InsertFeature(CancellationToken cancellationToken)
            => throw new NotSupportedException("Creation of this feature is not supported");

        protected virtual void CommitCache(IFeature feat, CancellationToken cancellationToken)
        {
        }

        public ISwDimensionsCollection Dimensions => m_DimensionsLazy.Value;

        public string Name 
        {
            get
            {
                if (IsCommitted)
                {
                    return Feature.Name;
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<string>();
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    Feature.Name = value;
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }
        
        public Color? Color
        {
            get
            {
                if (IsCommitted)
                {
                    return GetColor(Feature);
                }
                else
                {
                    return m_Creator.CachedProperties.Get<Color?>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    SetColor(Feature, value);
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        private Color? GetColor(IFeature feat) => SwColorHelper.GetColor((IComponent2)((IEntity)feat).GetComponent(),
            (o, c) => feat.GetMaterialPropertyValues2((int)o, c) as double[]);

        private void SetColor(IFeature feat, Color? color)=> SwColorHelper.SetColor(color, (IComponent2)((IEntity)feat).GetComponent(),
                (m, o, c) => feat.SetMaterialPropertyValues2(m, (int)o, c),
                (o, c) => feat.RemoveMaterialProperty2((int)o, c));

        public override bool IsCommitted => m_Creator.IsCreated;

        public virtual FeatureState_e State 
        {
            get
            {
                var state = FeatureState_e.Default;
                
                GetConfigurationOptions(out var confOpts, out var confNames);

                var suppStates = (bool[])Feature.IsSuppressed2((int)confOpts, confNames);

                if (suppStates[0])
                {
                    state |= FeatureState_e.Suppressed;
                }

                return state;
            }
            set 
            {
                swFeatureSuppressionAction_e action;

                if (value.HasFlag(FeatureState_e.Suppressed))
                {
                    action = swFeatureSuppressionAction_e.swSuppressFeature;
                }
                else 
                {
                    action = swFeatureSuppressionAction_e.swUnSuppressFeature;
                }

                GetConfigurationOptions(out var confOpts, out var confNames);

                if (!Feature.SetSuppression2((int)action, (int)confOpts, confNames)) 
                {
                    throw new Exception("Failed to change the suppresion of the feature");
                }
            }
        }

        private void GetConfigurationOptions(out swInConfigurationOpts_e confOpts, out string[] confNames)
        {
            var context = GetContext();

            switch (context.Owner)
            {
                case ISwComponent comp:
                    confOpts = swInConfigurationOpts_e.swSpecifyConfiguration;
                    confNames = new string[] { comp.ReferencedConfiguration.Name };
                    break;

                case ISwConfiguration conf:
                    confOpts = swInConfigurationOpts_e.swSpecifyConfiguration;
                    confNames = new string[] { conf.Name };
                    break;

                default:
                    confOpts = swInConfigurationOpts_e.swThisConfiguration;
                    confNames = null;
                    break;
            }
        }

        public virtual bool IsUserFeature => Array.IndexOf(m_SolderedFeatureTypes, Feature.GetTypeName2()) == -1;

        public IEntity Entity => (IEntity)Feature;

        public ISwEntityRepository AdjacentEntities { get; }

        public virtual ISwBody Body 
        {
            get 
            {
                var bodies = AdjacentEntities.Cast<ISwEntity>().Select(e => e.Body).Distinct(new XObjectEqualityComparer<ISwBody>()).ToArray();

                if (bodies.Length == 1)
                {
                    return bodies.First();
                }
                else if (bodies.Length == 0)
                {
                    throw new Exception("This feature does not have bodies");
                }
                else
                {
                    throw new Exception("This feature has multiple bodies");
                }
            }
        }

        internal override void Select(bool append, ISelectData selData)
        {
            if (!Feature.Select2(append, selData?.Mark ?? 0))
            {
                throw new Exception("Faile to select feature");
            }
        }

        public virtual IEditor<IXFeature> Edit() => throw new NotSupportedException();

        public XCad.Geometry.Structures.Point FindClosestPoint(XCad.Geometry.Structures.Point point)
            => throw new NotSupportedException();

        protected override bool IsSameDispatch(object disp)
        {
            if (OwnerApplication.Sw.IsSame(disp, Dispatch) == (int)swObjectEquality.swObjectSame)
            {
                return true;
            }
            else 
            {
                //NOTE: some of the features override the dispatch to be a specific feature (e.g. RefPlane)
                //this results in different pointers when comparing and some of the methods (like IsSelected) may incorrectly
                //compare the pointers and return unexpected result depending on the method feature was selected (e.g. Feature::Select selects IFeature, while SelectByID2 selected IRefPlane)
                if (Dispatch != Feature)
                {
                    return OwnerApplication.Sw.IsSame(disp, Feature) == (int)swObjectEquality.swObjectSame;
                }
                else 
                {
                    return false;
                }
            }
        }

        private Context GetContext()
        {
            if (m_Context == null)
            {
                var comp = ((IEntity)Feature).GetComponent();

                if (comp != null)
                {
                    m_Context = new Context(OwnerDocument.CreateObjectFromDispatch<ISwComponent>(comp));
                }
                else
                {
                    m_Context = new Context(OwnerDocument);
                }
            }

            return m_Context;
        }
    }
}
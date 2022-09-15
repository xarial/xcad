//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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

namespace Xarial.XCad.SolidWorks.Features
{
    public interface ISwFeature : ISwSelObject, IXFeature
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
                    throw new Exception("Failed to modify defintion of the feature");
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
            "OriginProfileFeature",
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

        IXComponent IXFeature.Component => Component;
        IXDimensionRepository IDimensionable.Dimensions => Dimensions;

        protected readonly IElementCreator<IFeature> m_Creator;

        public virtual IFeature Feature => m_Creator.Element;

        public override object Dispatch => Feature;

        public override bool IsAlive => this.CheckIsAlive(() => Feature.GetID());

        private readonly Lazy<SwFeatureDimensionsCollection> m_DimensionsLazy;
        private Context m_Context;

        internal SwFeature(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(feat, doc, app)
        {
            if (doc == null) 
            {
                throw new ArgumentNullException(nameof(doc));
            }

            m_DimensionsLazy = new Lazy<SwFeatureDimensionsCollection>(() => 
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

                return new SwFeatureDimensionsCollection(this, OwnerDocument, m_Context);
            });

            m_Creator = new ElementCreator<IFeature>(CreateFeature, CommitCache, feat, created);
        }

        internal void SetContext(Context context) 
        {
            m_Context = context;
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

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
            using (var viewFreeze = new ViewFreeze(OwnerDocument))
            {
                var feat = InsertFeature(cancellationToken);

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

        public IEnumerable<IXFace> Faces 
        {
            get 
            {
                var faces = (object[])Feature.GetFaces();

                if (faces != null)
                {
                    foreach (var face in faces) 
                    {
                        yield return OwnerDocument.CreateObjectFromDispatch<ISwFace>(face);
                    }
                }
            }
        }

        public FeatureState_e State 
        {
            get 
            {
                var state = FeatureState_e.Default;

                var suppStates = (bool[])Feature.IsSuppressed2((int)swInConfigurationOpts_e.swThisConfiguration, null);

                if (suppStates[0]) 
                {
                    state |= FeatureState_e.Suppressed;
                }

                return state;
            }
        }

        public virtual bool IsUserFeature => Array.IndexOf(m_SolderedFeatureTypes, Feature.GetTypeName2()) == -1;

        internal override void Select(bool append, ISelectData selData)
        {
            if (!Feature.Select2(append, selData?.Mark ?? 0))
            {
                throw new Exception("Faile to select feature");
            }
        }

        public virtual IEditor<IXFeature> Edit() => throw new NotSupportedException();
    }
}
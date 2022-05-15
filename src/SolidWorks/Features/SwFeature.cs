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
        IXComponent IXFeature.Component => Component;
        IXDimensionRepository IDimensionable.Dimensions => Dimensions;

        private readonly ElementCreator<IFeature> m_Creator;

        public virtual IFeature Feature => m_Creator.Element;

        public override object Dispatch => Feature;

        private readonly Lazy<SwFeatureDimensionsCollection> m_DimensionsLazy;
        private Context m_Context;

        internal SwFeature(IFeature feat, ISwDocument doc, ISwApplication app, bool created) : base(feat, doc, app)
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

            m_Creator = new ElementCreator<IFeature>(CreateFeature, feat, created);
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

        protected virtual IFeature CreateFeature(CancellationToken cancellationToken)
            => throw new NotSupportedException("Creation of this feature is not supported");

        public ISwDimensionsCollection Dimensions => m_DimensionsLazy.Value;

        public string Name 
        {
            get => Feature.Name;
            set => Feature.Name = value;
        }
        
        public Color? Color
        {
            get => SwColorHelper.GetColor(Component?.Component,
                (o, c) => Feature.GetMaterialPropertyValues2((int)o, c) as double[]);
            set => SwColorHelper.SetColor(value, Component?.Component,
                (m, o, c) => Feature.SetMaterialPropertyValues2(m, (int)o, c),
                (o, c) => Feature.RemoveMaterialProperty2((int)o, c));
        }

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
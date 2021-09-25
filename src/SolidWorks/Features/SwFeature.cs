//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using Xarial.XCad.Annotations;
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
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class SwFeature : SwSelObject, ISwFeature
    {
        private readonly ElementCreator<IFeature> m_Creator;

        IXDimensionRepository IXFeature.Dimensions => Dimensions;

        public virtual IFeature Feature
            => m_Creator.Element;

        public override object Dispatch => Feature;

        internal SwFeature(IFeature feat, ISwDocument doc, ISwApplication app, bool created) : base(feat, doc, app)
        {
            if (doc == null) 
            {
                throw new ArgumentNullException(nameof(doc));
            }

            Dimensions = new SwFeatureDimensionsCollection(this, OwnerDocument, OwnerApplication);

            m_Creator = new ElementCreator<IFeature>(CreateFeature, feat, created);
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        protected virtual IFeature CreateFeature(CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Creation of this feature is not supported");
        }

        public ISwDimensionsCollection Dimensions { get; }

        public string Name 
        {
            get => Feature.Name;
            set => Feature.Name = value;
        }

        private IComponent2 Component => (Feature as IEntity).GetComponent() as IComponent2;

        public Color? Color
        {
            get => SwColorHelper.GetColor(Component,
                (o, c) => Feature.GetMaterialPropertyValues2((int)o, c) as double[]);
            set => SwColorHelper.SetColor(value, Component,
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

        public override void Select(bool append)
        {
            if (!Feature.Select2(append, 0))
            {
                throw new Exception("Faile to select feature");
            }
        }
    }
}
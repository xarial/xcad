//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Drawing;
using Xarial.XCad.Annotations;
using Xarial.XCad.Features;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Features
{
    public interface ISwFeature : IXFeature
    {
        IFeature Feature { get; }
        new ISwDimensionsCollection Dimensions { get; }
    }

    internal class SwFeature : SwSelObject, ISwFeature
    {
        private readonly ElementCreator<IFeature> m_Creator;

        IXDimensionRepository IXFeature.Dimensions => Dimensions;

        public IFeature Feature
        {
            get
            {
                return m_Creator.Element;
            }
        }
        
        private readonly ISwDocument m_Doc;

        internal SwFeature(ISwDocument doc, IFeature feat, bool created) : base(doc.Model, feat)
        {
            if (doc == null) 
            {
                throw new ArgumentNullException(nameof(doc));
            }

            m_Doc = doc;
            Dimensions = new SwFeatureDimensionsCollection(m_Doc, this);

            m_Creator = new ElementCreator<IFeature>(CreateFeature, feat, created);
        }

        public override void Commit() => m_Creator.Create();

        protected virtual IFeature CreateFeature()
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
            get => SwColorHelper.GetColor(Feature, Component,
                (o, c) => Feature.GetMaterialPropertyValues2((int)o, c) as double[]);
            set => SwColorHelper.SetColor(Feature, value, Component,
                (m, o, c) => Feature.SetMaterialPropertyValues2(m, (int)o, c),
                (o, c) => Feature.RemoveMaterialProperty2((int)o, c));
        }

        public override bool IsCommitted => m_Creator.IsCreated;

        public override void Select(bool append)
        {
            if (!Feature.Select2(append, 0))
            {
                throw new Exception("Faile to select feature");
            }
        }
    }
}
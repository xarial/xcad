//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents.EventHandlers;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.Utils.Diagnostics;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwPart : ISwDocument3D, IXPart 
    {
        IPartDoc Part { get; }
        new ISwPartConfigurationCollection Configurations { get; }
    }

    internal class SwPart : SwDocument3D, ISwPart
    {
        IXPartConfigurationRepository IXPart.Configurations => (IXPartConfigurationRepository)Configurations;

        public IPartDoc Part => Model as IPartDoc;

        public IXBodyRepository Bodies { get; }

        internal SwPart(IPartDoc part, SwApplication app, IXLogger logger, bool isCreated)
            : base((IModelDoc2)part, app, logger, isCreated)
        {
            Bodies = new SwPartBodyCollection(this);
            Evaluation = new SwPartEvaluation(this);
        }

        public IXMaterial Material
        {
            get => GetMaterial("");
            set => SetMaterial(value, "");
        }

        internal IXMaterial GetMaterial(string confName) 
        {
            var materialName = Part.GetMaterialPropertyName2(confName, out var database);

            if (!string.IsNullOrEmpty(materialName))
            {
                if (!OwnerApplication.MaterialDatabases.TryGet(database, out var db))
                {
                    db = null;
                }

                return new SwMaterial(materialName, (SwMaterialsDatabase)db);
            }
            else
            {
                return null;
            }
        }

        internal void SetMaterial(IXMaterial material, string confName) 
        {
            if (material != null)
            {
                Part.SetMaterialPropertyName2(confName, material.Database.Name, material.Name);
            }
            else
            {
                Part.SetMaterialPropertyName2(confName, "", "");
            }
        }

        internal protected override swDocumentTypes_e? DocumentType => swDocumentTypes_e.swDocPART;

        protected override bool IsLightweightMode => false;
        protected override bool IsRapidMode => false;

        protected override SwAnnotationCollection CreateAnnotations() => new SwDocument3DAnnotationCollection(this);

        ISwPartConfigurationCollection ISwPart.Configurations => (ISwPartConfigurationCollection)Configurations;

        public override IXDocumentEvaluation Evaluation { get; }
        
        protected override SwConfigurationCollection CreateConfigurations()
            => new SwPartConfigurationCollection(this, OwnerApplication);

        protected override bool IsDocumentTypeCompatible(swDocumentTypes_e docType) => docType == swDocumentTypes_e.swDocPART;
    }

    internal class SwPartBodyCollection : SwBodyCollection
    {
        private SwPart m_Part;

        public SwPartBodyCollection(SwPart rootDoc) : base(rootDoc)
        {
            m_Part = rootDoc;
        }

        protected override IEnumerable<IBody2> SelectSwBodies(swBodyType_e bodyType)
            => (m_Part.Part.GetBodies2((int)bodyType, false) as object[])?.Cast<IBody2>();
    }
}
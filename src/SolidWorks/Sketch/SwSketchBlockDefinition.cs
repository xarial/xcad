﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Xarial.XCad.Data;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public interface ISwSketchBlockDefinition : IXSketchBlockDefinition, ISwFeature
    {
        ISketchBlockDefinition SketchBlockDefinition { get; }
    }

    internal class SwSketchBlockDefinition : SwFeature, ISwSketchBlockDefinition
    {
        public ISketchBlockDefinition SketchBlockDefinition { get; }

        public IEnumerable<IXSketchBlockInstance> Instances 
        {
            get 
            {
                var instances = (object[])SketchBlockDefinition.GetInstances() ?? new object[0];

                foreach (ISketchBlockInstance inst in instances) 
                {
                    yield return OwnerDocument.CreateObjectFromDispatch<ISwSketchBlockInstance>(inst);
                }
            }
        }

        public IXSketchEntityRepository Entities { get; }

        public override bool IsAlive => this.CheckIsAlive(() => { var test = SketchBlockDefinition.LinkToFile; });

        public Point InsertionPoint => new Point((double[])SketchBlockDefinition.InsertionPoint.ArrayData);

        internal SwSketchBlockDefinition(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(GetSketchBlockDefinitionFeature(doc.Model, feat.Name), doc, app, created) 
        {
            SketchBlockDefinition = (ISketchBlockDefinition)feat.GetSpecificFeature2();

            Entities = new SwSketchEntityCollection(doc.CreateObjectFromDispatch<SwSketchBase>(SketchBlockDefinition.GetSketch()), doc, app);
        }

        //Note: retrieving the pointer to the feature from the feature tree for the consistency as IFeature retrieved from ISketchBlockDefinition has a different pointer to IFeature in the tree
        private static IFeature GetSketchBlockDefinitionFeature(IModelDoc2 model, string name) 
        {
            switch (model) 
            {
                case IPartDoc part:
                    return (IFeature)part.FeatureByName(name);

                case IAssemblyDoc assm:
                    return (IFeature)assm.FeatureByName(name);

                case IDrawingDoc drw:
                    return (IFeature)drw.FeatureByName(name);

                default:
                    throw new NotSupportedException();
            }
        }
    }
}

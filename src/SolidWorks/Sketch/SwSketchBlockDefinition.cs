﻿using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Xarial.XCad.Data;
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
        
        internal SwSketchBlockDefinition(IFeature feat, ISwDocument doc, ISwApplication app, bool created) : base(feat, doc, app, created) 
        {
            SketchBlockDefinition = (ISketchBlockDefinition)feat.GetSpecificFeature2();

            Entities = new SwSketchEntityCollection(doc.CreateObjectFromDispatch<ISwSketchBase>(SketchBlockDefinition.GetSketch()), doc, app);
        }
    }
}
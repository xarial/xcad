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

        //Note: retrieving the pointer to the feature from the feature tree for the consistency as IFeature retrieved from ISketchBlockDefinition has a different pointer to IFeature in the tree
        internal SwSketchBlockDefinition(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(doc.Features[feat.Name].Feature, doc, app, created) 
        {
            SketchBlockDefinition = (ISketchBlockDefinition)feat.GetSpecificFeature2();

            Entities = new SwSketchEntityCollection(doc.CreateObjectFromDispatch<SwSketchBase>(SketchBlockDefinition.GetSketch()), doc, app);
        }
    }
}

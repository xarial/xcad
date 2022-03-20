using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Features;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public interface ISwSketchBlockInstance : IXSketchBlockInstance, ISwFeature, ISwSketchEntity
    {
        ISketchBlockInstance SketchBlockInstance { get; }
    }

    internal class SwSketchBlockInstance : SwFeature, ISwSketchBlockInstance
    {
        public ISketchBlockInstance SketchBlockInstance { get; }
        public IXSketchBlockDefinition Definition => OwnerDocument.CreateObjectFromDispatch<ISwSketchBlockDefinition>(SketchBlockInstance.Definition);
        public IXSketchBase OwnerSketch => OwnerDocument.CreateObjectFromDispatch<ISwSketchBase>(SketchBlockInstance.GetSketch());
        
        public IXSketchBlockInstance OwnerBlock 
        {
            get 
            {
                foreach (var node in this.IterateAllSketchBlockInstanceNodes()) 
                {
                    var feat = (IFeature)node.Object;
                    var block = (ISketchBlockInstance)feat.GetSpecificFeature2();

                    if (OwnerApplication.Sw.IsSame(block, SketchBlockInstance) == (int)swObjectEquality.swObjectSame) 
                    {
                        var parentNode = node.GetParent();

                        if (parentNode.ObjectType == (int)swTreeControlItemType_e.swFeatureManagerItem_Feature)
                        {
                            var parentFeat = (IFeature)parentNode.Object;

                            if (parentFeat.GetTypeName2() == "SketchBlockInst")
                            {
                                return OwnerDocument.CreateObjectFromDispatch<ISwSketchBlockInstance>(parentFeat);
                            }
                            else 
                            {
                                return null;
                            }
                        }
                    }
                }

                throw new Exception("Sketch block instance is not found in the tree. This may indicate that tree is hidden or not loaded");
            }
        }

        internal SwSketchBlockInstance(IFeature feat, ISwDocument doc, ISwApplication app, bool created) : base(feat, doc, app, created)
        {
            SketchBlockInstance = (ISketchBlockInstance)feat.GetSpecificFeature2();
        }
    }
}

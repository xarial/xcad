using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Utils;

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
                if (AssignedOwnerBlock != null)
                {
                    return AssignedOwnerBlock;
                }
                else 
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
        }

        internal SwSketchBlockInstance AssignedOwnerBlock { get; set; }

        public TransformMatrix Transform => SketchBlockInstance.BlockToSketchTransform.ToTransformMatrix();

        public IXSketchEntityRepository Entities { get; }

        internal SwSketchBlockInstance(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(feat, doc, app, created)
        {
            SketchBlockInstance = (ISketchBlockInstance)feat.GetSpecificFeature2();
            Entities = new SwSketchBlockInstanceEntityCollection(this, doc.CreateObjectFromDispatch<SwSketchBase>(SketchBlockInstance.Definition.GetSketch()), doc, app);
        }

        public override bool Equals(IXObject other)
        {
            if (base.Equals(other))
            {
                //NOTE: sketch block instance pointers are from the definition and will be equal from different sketch block instances
                if (AssignedOwnerBlock != null && (other as SwSketchBlockInstance)?.AssignedOwnerBlock != null)
                {
                    return AssignedOwnerBlock.Equals(((SwSketchBlockInstance)other).AssignedOwnerBlock);
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
    }

    internal class SwSketchBlockInstanceEntityCollection : SwSketchEntityCollection
    {
        private readonly SwSketchBlockInstance m_SketchBlockInst;

        internal SwSketchBlockInstanceEntityCollection(SwSketchBlockInstance skBlockInst, SwSketchBase sketch, SwDocument doc, SwApplication app)
            : base(sketch, doc, app)
        {
            m_SketchBlockInst = skBlockInst;
        }

        protected override IEnumerable<ISwSketchEntity> IterateEntities()
        {
            foreach (var ent in base.IterateEntities()) 
            {
                switch (ent) 
                {
                    case SwSketchEntity skEnt:
                        skEnt.AssignedOwnerBlock = m_SketchBlockInst;
                        break;

                    case SwSketchBlockInstance skBlockInst:
                        skBlockInst.AssignedOwnerBlock = m_SketchBlockInst;
                        break;

                    default:
                        throw new NotSupportedException($"{ent?.GetType()} sketch block entity is not supported");
                }

                yield return ent;
            }
        }
    }
}

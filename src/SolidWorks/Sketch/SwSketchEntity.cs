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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Services;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public interface ISwSketchEntity : IXSketchEntity, ISwSelObject
    {
    }

    //TODO: for the entities of the block definition it might be required to transform all the coodinate system for the consistency
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal abstract class SwSketchEntity : SwSelObject, ISwSketchEntity
    {
        public abstract IXIdentifier Id { get; }
        public new abstract bool IsCommitted { get; }
        public abstract Color? Color { get; set; }

        public abstract IXSketchBase OwnerSketch { get; }

        public virtual IXSketchBlockInstance OwnerBlock 
        {
            get 
            {
                if (AssignedOwnerBlock != null)
                {
                    return AssignedOwnerBlock;
                }
                else
                {
                    var name = GetFullName();

                    var nameParts = name.Split('/');

                    if (nameParts.Length > 1)
                    {
                        var blockName = nameParts[1].Split('@').First();

                        var skBlockInstNode = this.IterateAllSketchBlockInstanceNodes().FirstOrDefault(
                            x => string.Equals(((IFeature)x.Object).Name, blockName, StringComparison.CurrentCultureIgnoreCase));

                        if (skBlockInstNode != null)
                        {
                            return OwnerDocument.CreateObjectFromDispatch<ISwSketchBlockInstance>(skBlockInstNode.Object);
                        }
                        else
                        {
                            throw new Exception($"Failed to find the sketch block instance with the name '{blockName}'");
                        }
                    }

                    return null;
                }
            }
        }

        internal SwSketchBlockInstance AssignedOwnerBlock { get; set; }

        public string Name
        {
            get => GetFullName();
            set => throw new NotSupportedException();
        }
        public abstract IXLayer Layer { get; set; }

        internal SwSketchEntity(object ent, SwDocument doc, SwApplication app) : base(ent, doc, app)
        {
        }

        public override bool Equals(IXObject other)
        {
            if (base.Equals(other))
            {
                //NOTE: sketch segment pointers are from the definition and will be equal from different sketch block instances
                if (AssignedOwnerBlock != null && (other as SwSketchEntity)?.AssignedOwnerBlock != null)
                {
                    return AssignedOwnerBlock.Equals(((SwSketchEntity)other).AssignedOwnerBlock);
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

        protected abstract string GetFullName();
    }

    internal static class SwSketchEntityExtension 
    {
        internal static IEnumerable<ITreeControlItem> IterateAllSketchBlockInstanceNodes(this ISwSketchEntity skEnt)
        {
            var app = ((SwObject)skEnt).OwnerApplication.Sw;
            var model = ((SwObject)skEnt).OwnerDocument.Model;
            var sketchFeat = ((SwSketchBase)skEnt.OwnerSketch).Feature;

            var sketchNode = FindTreeNode(model, sketchFeat, app);

            foreach (var node in EnumerateTreeNodes(sketchNode, _ => true))
            {
                if (node.ObjectType == (int)swTreeControlItemType_e.swFeatureManagerItem_Feature)
                {
                    var feat = (IFeature)node.Object;

                    if (feat.GetTypeName2() == "SketchBlockInst")
                    {
                        yield return node;
                    }
                }
            }
        }

        private static ITreeControlItem FindTreeNode(IModelDoc2 model, IFeature feat, ISldWorks app) 
        {
            var originPassed = false;

            foreach (var node in EnumerateTreeNodes(model.FeatureManager.GetFeatureTreeRootItem2((int)swFeatMgrPane_e.swFeatMgrPaneBottom), _ => originPassed)) 
            {
                if (originPassed) 
                {
                    if (IsSketchNode(node, feat, app)) 
                    {
                        return node;
                    }
                }

                originPassed = originPassed || IsOrigin(node);
            }

            throw new Exception($"Tree node is not found for {feat.Name}");
        }

        private static IEnumerable<ITreeControlItem> EnumerateTreeNodes(ITreeControlItem parent, Predicate<ITreeControlItem> traverseChildPred)
        {
            var child = parent.GetFirstChild();

            while (child != null)
            {
                yield return child;

                if (traverseChildPred.Invoke(child))
                {
                    foreach (var subChild in EnumerateTreeNodes(child, traverseChildPred))
                    {
                        yield return subChild;
                    }
                }

                child = child.GetNext();
            }
        }
        
        private static bool IsSketchNode(ITreeControlItem node, IFeature sketchFeat, ISldWorks app)
        {
            if (node.ObjectType == (int)swTreeControlItemType_e.swFeatureManagerItem_Feature)
            {
                var feat = (IFeature)node.Object;

                var typeName = feat.GetTypeName2();

                if (typeName == SwSketch2D.TypeName || typeName == SwSketch3D.TypeName)
                {
                    return feat.Name.Equals(sketchFeat.Name);
                    //return app.IsSame(feat, sketchFeat) == (int)swObjectEquality.swObjectSame;
                }
            }

            return false;
        }

        private static bool IsOrigin(ITreeControlItem node) 
        {
            if (node.ObjectType == (int)swTreeControlItemType_e.swFeatureManagerItem_Feature)
            {
                var feat = (IFeature)node.Object;

                var typeName = feat.GetTypeName2();

                return typeName == SwOrigin.TypeName;
            }

            return false;
        }
    }
}
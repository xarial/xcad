﻿//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

    internal abstract class SwSketchEntity : SwSelObject, ISwSketchEntity
    {
        public new abstract bool IsCommitted { get; }
        public abstract Color? Color { get; set; }

        public abstract IXSketchBase OwnerSketch { get; }

        public IXSketchBlockInstance OwnerBlock 
        {
            get 
            {
                var name = GetName();

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

        internal SwSketchEntity(object ent, ISwDocument doc, ISwApplication app) : base(ent, doc, app)
        {
        }

        protected abstract string GetName();
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

                originPassed = IsOrigin(node);
            }

            throw new Exception($"Tree node is not found for {feat.Name}");

            //var child = parent.GetFirstChild();

            //while (child != null)
            //{
            //    if (originPassed)
            //    {
            //        bool? isSketch = null;

            //        if (!isWithinSketch)
            //        {
            //            isSketch = IsSketchNode(child, sketchFeat, app);
            //        }

            //        isWithinSketch = isWithinSketch || isSketch.Value;

            //        foreach (var subChild in TraverseSketchChildrenNodes(child, sketchFeat, app, isWithinSketch, originPassed))
            //        {
            //            if (isWithinSketch)
            //            {
            //                yield return subChild;
            //            }
            //        }

            //        if (isSketch.HasValue && isSketch.Value)
            //        {
            //            isWithinSketch = false;
            //            yield break;
            //        }
            //    }

            //    originPassed = originPassed || IsOrigin(child);

            //    child = child.GetNext();
            //}
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

                if (typeName == "ProfileFeature" || typeName == "3DProfileFeature")
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

                return typeName == "OriginProfileFeature";
            }

            return false;
        }
    }
}
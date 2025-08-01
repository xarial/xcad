//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using Xarial.XCad.Documents;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Services;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Sketch
{
    /// <summary>
    /// SOLIDWORKS specific sketch block instance
    /// </summary>
    public interface ISwSketchBlockInstance : IXSketchBlockInstance, ISwFeature, ISwSketchEntity
    {
        /// <summary>
        /// Pointer to sketch block instance
        /// </summary>
        ISketchBlockInstance SketchBlockInstance { get; }
    }

    internal class SwSketchBlockInstance : SwFeature, ISwSketchBlockInstance
    {
        public ISketchBlockInstance SketchBlockInstance => (ISketchBlockInstance)Feature.GetSpecificFeature2();
        
        public IXSketchBlockDefinition Definition
        {
            get
            {
                if (IsCommitted)
                {
                    return OwnerDocument.CreateObjectFromDispatch<ISwSketchBlockDefinition>(SketchBlockInstance.Definition);
                }
                else 
                {
                    return Creator.CachedProperties.Get<IXSketchBlockDefinition>();
                }
            }
            private set 
            {
                if (IsCommitted)
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
                else 
                {
                    Creator.CachedProperties.Set(value);
                }
            }
            
        }

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

        public IXLayer Layer
        {
            get => SwLayerHelper.GetLayer(this, x => x.SketchBlockInstance.Layer);
            set => SwLayerHelper.SetLayer(this, value, (x, y) => x.SketchBlockInstance.Layer = y);
        }

        public TransformMatrix Transform
        {
            get
            {
                if (IsCommitted)
                {
                    return SketchBlockInstance.BlockToSketchTransform.ToTransformMatrix();
                }
                else
                {
                    return Creator.CachedProperties.Get<TransformMatrix>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    ParseTransform(value, out var pos, out var angle, out var scale);

                    var lockAngle = SketchBlockInstance.LockAngle;

                    SketchBlockInstance.InstancePosition = pos;
                    SketchBlockInstance.LockAngle = false;
                    SketchBlockInstance.Angle = angle;
                    SketchBlockInstance.Scale2 = scale;
                    SketchBlockInstance.LockAngle = lockAngle;
                }
                else
                {
                    Creator.CachedProperties.Set(value);
                }
            }
        }

        public override bool IsAlive => this.CheckIsAlive(() => 
        {
            var test = SketchBlockInstance.Name;

            //NOTE: the deleted block may still produce a valid pointer and all the methods can be executed successfully, checking if the definition still contains this block
            var instances = (object[])SketchBlockInstance.Definition.GetInstances();

            if (instances?.Any(i => OwnerApplication.Sw.IsSame(i, SketchBlockInstance) == (int)swObjectEquality.swObjectSame) != true)
            {
                throw new Exception();
            }
        });

        public IXSketchEntityRepository Entities { get; }

        private readonly SwSketchBase m_Sketch;

        internal SwSketchBlockInstance(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(feat, doc, app, created)
        {
            if (created)
            {
                m_Sketch = doc.CreateObjectFromDispatch<SwSketchBase>(SketchBlockInstance.Definition.GetSketch());
            }
            else
            {
                m_Sketch = doc.Features.PreCreate<SwSketch2D>();
            }

            Entities = new SwSketchBlockInstanceEntityCollection(this, m_Sketch, doc, app);
        }

        internal SwSketchBlockInstance(SwSketchBlockDefinition skBlockDef, SwDocument doc, SwApplication app) : this(default(IFeature), doc, app, false)
        {
            Definition = skBlockDef;
        }

        protected override IFeature InsertFeature(CancellationToken cancellationToken)
        {
            if (Definition != null)
            {
                ParseTransform(Transform, out var pos, out var angle, out var scale);

                var skBlockInst = OwnerDocument.Model.SketchManager.InsertSketchBlockInstance((SketchBlockDefinition)((ISwSketchBlockDefinition)Definition).SketchBlockDefinition, pos, scale, angle);

                if (skBlockInst != null)
                {
                    m_Sketch.Creator.Set((IFeature)skBlockInst.Definition.GetSketch());

                    return (IFeature)skBlockInst;
                }
                else 
                {
                    throw new Exception("Failed to insert sketch block instance");
                }
            }
            else 
            {
                throw new Exception("Sketch block definition is not set");
            }
        }

        private void ParseTransform(TransformMatrix transform, out MathPoint pos, out double angle, out double scale)
        {
            if (transform == null)
            {
                transform = TransformMatrix.Identity;
            }

            scale = transform.Scale.X;
            transform.GetEulerAngles(out angle, out _, out _);

            //need clockwise
            angle *= -1;

            var mathUtils = OwnerApplication.Sw.IGetMathUtility();

            pos = (MathPoint)mathUtils.CreatePoint((Definition.InsertionPoint * transform).ToArray());
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
        private readonly SwDocument m_Doc;

        internal SwSketchBlockInstanceEntityCollection(SwSketchBlockInstance skBlockInst, SwSketchBase sketch, SwDocument doc, SwApplication app)
            : base(sketch, doc, app)
        {
            m_SketchBlockInst = skBlockInst;
            m_Doc = doc;
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

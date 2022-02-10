//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Exceptions;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwBoundingBox : IXBoundingBox 
    {
    }

    public interface ISwAssemblyBoundingBox : ISwBoundingBox, IXAssemblyBoundingBox
    {
    }

    internal abstract class SwBoundingBox : ISwBoundingBox
    {
        internal class EditableBox3D 
        {
            internal Box3D Box { get; set; }
        }

        private readonly IMathUtility m_MathUtils;

        protected readonly ElementCreator<EditableBox3D> m_Creator;

        private readonly ISwDocument m_Doc;

        internal SwBoundingBox(ISwDocument doc, ISwApplication app) 
        {
            m_Doc = doc;

            m_MathUtils = app.Sw.IGetMathUtility();

            m_Creator = new ElementCreator<EditableBox3D>(CreateBox, null, false);

            UserUnits = false;
        }

        public Box3D Box => m_Creator.Element.Box;

        public TransformMatrix RelativeTo 
        {
            get => m_Creator.CachedProperties.Get<TransformMatrix>();
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommittedElementPropertyChangeNotSupported();
                }
            }
        }

        public bool Precise
        {
            get => m_Creator.CachedProperties.Get<bool>();
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommittedElementPropertyChangeNotSupported();
                }
            }
        }

        public bool VisibleOnly
        {
            get => m_Creator.CachedProperties.Get<bool>();
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommittedElementPropertyChangeNotSupported();
                }
            }
        }

        public virtual IXBody[] Scope
        {
            get => m_Creator.CachedProperties.Get<IXBody[]>();
            set
            {
                m_Creator.CachedProperties.Set(value);

                if (IsCommitted)
                {
                    m_Creator.Element.Box = CalculateBoundingBox();
                }
            }
        }

        public bool IsCommitted => m_Creator.IsCreated;

        public void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);

        private EditableBox3D CreateBox(CancellationToken cancellationToken)
            => new EditableBox3D()
            {
                Box = CalculateBoundingBox()
            };

        protected Box3D CalculateBoundingBox()
        {
            if (Precise)
            {
                IXBody[] bodies;

                var scope = Scope;

                if (scope != null)
                {
                    bodies = scope;
                }
                else
                {
                    bodies = GetAllBodies();
                }

                if (bodies?.Any() != true)
                {
                    throw new EvaluationFailedException();
                }

                return ComputePreciseBoundingBox(bodies);
            }
            else
            {
                if (RelativeTo != null)
                {
                    throw new NotSupportedException("RelativeTo can only be calculated when precise bounding box is used");
                }

                double[] bbox;

                if (IsScoped)
                {
                    bbox = ComputeScopedApproximateBoundingBox();
                }
                else
                {
                    bbox = ComputeFullApproximateBoundingBox();
                }

                if (bbox.All(x => Math.Abs(x) < double.Epsilon))
                {
                    throw new EvaluationFailedException();
                }

                return CreateBoxFromData(bbox[0], bbox[1], bbox[2], bbox[3], bbox[4], bbox[5]);
            }
        }

        protected virtual bool IsScoped => Scope != null;

        public bool UserUnits
        {
            get => m_Creator.CachedProperties.Get<bool>();
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommittedElementPropertyChangeNotSupported();
                }
            }
        }

        protected abstract IXBody[] GetAllBodies();
        protected abstract double[] ComputeFullApproximateBoundingBox();

        protected virtual double[] ComputeScopedApproximateBoundingBox()
        {
            var bodies = Scope.Select(b => GetSwBody(b, out _)).ToArray();

            if (!bodies.Any()) 
            {
                throw new EvaluationFailedException();
            }

            var minX = double.MaxValue;
            var minY = double.MaxValue;
            var minZ = double.MaxValue;
            var maxX = double.MinValue;
            var maxY = double.MinValue;
            var maxZ = double.MinValue;

            foreach (var body in bodies)
            {
                var bodyBox = (double[])body.GetBodyBox();

                if (bodyBox[0] < minX)
                {
                    minX = bodyBox[0];
                }

                if (bodyBox[1] < minY)
                {
                    minY = bodyBox[1];
                }

                if (bodyBox[2] < minZ)
                {
                    minZ = bodyBox[2];
                }

                if (bodyBox[3] > maxX)
                {
                    maxX = bodyBox[3];
                }

                if (bodyBox[4] > maxY)
                {
                    maxY = bodyBox[4];
                }

                if (bodyBox[5] > maxZ)
                {
                    maxZ = bodyBox[5];
                }
            }

            return new double[] { minX, minY, minZ, maxX, maxY, maxZ };
        }

        protected Box3D ComputePreciseBoundingBox(IXBody[] bodies)
        {
            var minX = double.MaxValue;
            var minY = double.MaxValue;
            var minZ = double.MaxValue;
            var maxX = double.MinValue;
            var maxY = double.MinValue;
            var maxZ = double.MinValue;

            IMathTransform mathTransform = null;

            if (bodies.Any())
            {
                if (RelativeTo != null)
                {
                    mathTransform = m_MathUtils.ToMathTransform(RelativeTo);

                    bodies = bodies.Select(b =>
                    {
                        var swBody = GetSwBody(b, out bool isCopy);

                        if (!isCopy) 
                        {
                            swBody = swBody.ICopy();
                        }

                        if (!swBody.ApplyTransform(mathTransform.IInverse()))
                        {
                            throw new Exception("Failed to apply relative to coordinate");
                        }

                        return m_Doc.CreateObjectFromDispatch<SwBody>(swBody);
                    }).ToArray();
                }

                foreach (var body in bodies)
                {
                    var swBody = GetSwBody(body, out _);

                    double x;
                    double y;
                    double z;

                    swBody.GetExtremePoint(1, 0, 0, out x, out y, out z);

                    if (x > maxX)
                    {
                        maxX = x;
                    }

                    swBody.GetExtremePoint(-1, 0, 0, out x, out y, out z);

                    if (x < minX)
                    {
                        minX = x;
                    }

                    swBody.GetExtremePoint(0, 1, 0, out x, out y, out z);

                    if (y > maxY)
                    {
                        maxY = y;
                    }

                    swBody.GetExtremePoint(0, -1, 0, out x, out y, out z);

                    if (y < minY)
                    {
                        minY = y;
                    }

                    swBody.GetExtremePoint(0, 0, 1, out x, out y, out z);

                    if (z > maxZ)
                    {
                        maxZ = z;
                    }

                    swBody.GetExtremePoint(0, 0, -1, out x, out y, out z);

                    if (z < minZ)
                    {
                        minZ = z;
                    }
                }
            }

            return CreateBoxFromData(minX, minY, minZ, maxX, maxY, maxZ, mathTransform);
        }

        private IBody2 GetSwBody(IXBody srcBody, out bool isCopy)
        {
            var swBody = ((ISwBody)srcBody).Body;

            var comp = (IComponent2)(swBody.IGetFirstFace() as IEntity).GetComponent();

            if (comp != null)
            {
                swBody = swBody.ICopy();

                isCopy = true;

                if (!swBody.ApplyTransform(comp.Transform2))
                {
                    throw new Exception("Failed to apply component's transform");
                }
            }
            else 
            {
                isCopy = false;
            }

            return swBody;
        }

        private Box3D CreateBoxFromData(double minX, double minY, double minZ, double maxX, double maxY, double maxZ,
            IMathTransform mathTransform = null)
        {
            if (UserUnits)
            {
                var userUnit = m_Doc.Model.IGetUserUnit((int)swUserUnitsType_e.swLengthUnit);
                var unitConvFactor = userUnit.GetConversionFactor();

                minX = minX * unitConvFactor;
                minY = minY * unitConvFactor;
                minZ = minZ * unitConvFactor;
                maxX = maxX * unitConvFactor;
                maxY = maxY * unitConvFactor;
                maxZ = maxZ * unitConvFactor;
            }

            var width = maxX - minX;
            var height = maxY - minY;
            var length = maxZ - minZ;

            var centerPt = new Point((maxX + minX) / 2, (maxY + minY) / 2, (maxZ + minZ) / 2);

            var dirX = new Vector(1, 0, 0);
            var dirY = new Vector(0, 1, 0);
            var dirZ = new Vector(0, 0, 1);

            if (mathTransform != null)
            {
                var pt = (IMathPoint)m_MathUtils.CreatePoint(centerPt.ToArray());
                pt = (IMathPoint)pt.MultiplyTransform(mathTransform);
                centerPt = new Point((double[])pt.ArrayData);

                var vec = (IMathVector)m_MathUtils.CreateVector(dirX.ToArray());
                vec = (IMathVector)vec.MultiplyTransform(mathTransform);
                dirX = new Vector((double[])vec.ArrayData);

                vec = (IMathVector)m_MathUtils.CreateVector(dirY.ToArray());
                vec = (IMathVector)vec.MultiplyTransform(mathTransform);
                dirY = new Vector((double[])vec.ArrayData);

                vec = (IMathVector)m_MathUtils.CreateVector(dirZ.ToArray());
                vec = (IMathVector)vec.MultiplyTransform(mathTransform);
                dirZ = new Vector((double[])vec.ArrayData);
            }

            return new Box3D(width, height, length, centerPt, dirX, dirY, dirZ);
        }

        public void Dispose()
        {
        }
    }

    internal class SwAssemblyBoundingBox : SwBoundingBox, ISwAssemblyBoundingBox
    {
        private readonly ISwAssembly m_Assm;

        internal SwAssemblyBoundingBox(ISwAssembly assm, ISwApplication app) : base(assm, app)
        {
            m_Assm = assm;
            VisibleOnly = true;
        }

        protected override bool IsScoped
            => (this as IXAssemblyBoundingBox).Scope != null || base.Scope != null;

        public override IXBody[] Scope 
        {
            get 
            {
                var comps = (this as IXAssemblyBoundingBox).Scope;
                
                if (comps == null)
                {
                    return base.Scope;
                }
                else 
                {
                    return comps.SelectMany(c => c.IterateBodies(!VisibleOnly)).ToArray();
                }
            }
            set => base.Scope = value; 
        }

        IXComponent[] IAssemblyEvaluation.Scope
        {
            get => m_Creator.CachedProperties.Get<IXComponent[]>(nameof(Scope) + "_Components");
            set
            {
                m_Creator.CachedProperties.Set(value, nameof(Scope) + "_Components");

                if (IsCommitted)
                {
                    m_Creator.Element.Box = CalculateBoundingBox();
                }
            }
        }

        protected override double[] ComputeScopedApproximateBoundingBox()
        {
            if (!VisibleOnly) 
            {
                throw new NotSupportedException("Only avisible components can be considered when performing approximate bounding box calculation");
            }

            var comps = (this as IXAssemblyBoundingBox).Scope;

            if (comps != null)
            {
                if (!comps.Any()) 
                {
                    throw new EvaluationFailedException();
                }

                var minX = double.MaxValue;
                var minY = double.MaxValue;
                var minZ = double.MaxValue;
                var maxX = double.MinValue;
                var maxY = double.MinValue;
                var maxZ = double.MinValue;

                foreach (ISwComponent comp in comps)
                {
                    var compBox = (double[])comp.Component.GetBox(false, false);

                    if (compBox[0] < minX)
                    {
                        minX = compBox[0];
                    }

                    if (compBox[1] < minY)
                    {
                        minY = compBox[1];
                    }

                    if (compBox[2] < minZ)
                    {
                        minZ = compBox[2];
                    }

                    if (compBox[3] > maxX)
                    {
                        maxX = compBox[3];
                    }

                    if (compBox[4] > maxY)
                    {
                        maxY = compBox[4];
                    }

                    if (compBox[5] > maxZ)
                    {
                        maxZ = compBox[5];
                    }
                }

                return new double[] { minX, minY, minZ, maxX, maxY, maxZ };
            }
            else 
            {
                return base.ComputeScopedApproximateBoundingBox();
            }
        }

        protected override IXBody[] GetAllBodies()
            => m_Assm.Configurations.Active.Components.SelectMany(c => c.IterateBodies(!VisibleOnly)).ToArray();

        protected override double[] ComputeFullApproximateBoundingBox()
        {
            if (!VisibleOnly)
            {
                throw new NotSupportedException("Only avisible components can be considered when performing approximate bounding box calculation");
            }

            swBoundingBoxOptions_e bboxOptionsDefault = 0;

            return (double[])m_Assm.Assembly.GetBox((int)bboxOptionsDefault);
        }
    }

    internal class SwPartBoundingBox : SwBoundingBox
    {
        private readonly ISwPart m_Part;

        internal SwPartBoundingBox(ISwPart part, ISwApplication app) : base(part, app)
        {
            m_Part = part;
        }

        protected override double[] ComputeFullApproximateBoundingBox()
            => (double[])m_Part.Part.GetPartBox(true);

        protected override IXBody[] GetAllBodies()
            => m_Part.Bodies.OfType<IXSolidBody>()
                .Where(b => !VisibleOnly || b.Visible).ToArray();
    }
}

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
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Extensions;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Evaluation;
using Xarial.XCad.Geometry.Exceptions;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Evaluation
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

        protected readonly IElementCreator<EditableBox3D> m_Creator;

        private readonly ISwDocument m_Doc;

        private readonly SwApplication m_App;

        internal SwBoundingBox(SwDocument doc, SwApplication app)
        {
            m_Doc = doc;

            m_App = app;

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
                    m_Creator.Element.Box = CalculateBoundingBox(default);
                }
            }
        }

        public bool IsCommitted => m_Creator.IsCreated;

        public void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);

        private EditableBox3D CreateBox(CancellationToken cancellationToken)
            => new EditableBox3D()
            {
                Box = CalculateBoundingBox(cancellationToken)
            };

        protected Box3D CalculateBoundingBox(CancellationToken cancellationToken)
        {
            if (BestFit)
            {
                var bodies = Scope;

                if (bodies?.Any() != true)
                {
                    bodies = GetAllBodies();
                }

                if (bodies?.Any() != true)
                {
                    throw new EvaluationFailedException("No bodies found");
                }

                return ComputeBestFitBoundingBox(bodies, cancellationToken);
            }
            else
            {
                if (Precise)
                {
                    var bodies = Scope;

                    if (bodies?.Any() != true)
                    {
                        bodies = GetAllBodies();
                    }

                    if (bodies?.Any() != true)
                    {
                        throw new EvaluationFailedException("No bodies found");
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
        }

        private Box3D ComputeBestFitBoundingBox(IXBody[] bodies, CancellationToken cancellationToken)
        {
            if (m_App.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2018))
            {
                using (var tempPart = (ISwPart)m_App.Documents.PreCreatePart())
                {
                    tempPart.State |= DocumentState_e.Silent | DocumentState_e.Hidden;

                    tempPart.Commit(cancellationToken);

                    tempPart.Features.AddRange(bodies.Select(b =>
                    {
                        var swBody = GetTransformedSwBody(b, out var isCopy);

                        var targBody = (IXBody)m_Doc.CreateObjectFromDispatch<SwBody>(swBody);

                        if (!isCopy)
                        {
                            targBody = targBody.Copy();
                        }

                        var dumbBody = tempPart.Features.PreCreate<ISwDumbBody>();
                        dumbBody.BaseBody = targBody;
                        return dumbBody;
                    }));

                    tempPart.Rebuild();

                    ISwPlane refPlane;
                    swGlobalBoundingBoxFitOptions_e fitOpts;

                    if (RelativeTo == null)
                    {
                        fitOpts = swGlobalBoundingBoxFitOptions_e.swBoundingBoxType_BestFit;
                        refPlane = null;
                    }
                    else
                    {
                        fitOpts = swGlobalBoundingBoxFitOptions_e.swBoundingBoxType_CustomPlane;

                        var transform = RelativeTo;

                        var point = new Point(0, 0, 0).Transform(transform);
                        var normal = new Vector(0, 0, 1).Transform(transform);
                        var dir = new Vector(1, 0, 0).Transform(transform);

                        refPlane = tempPart.Features.PreCreate<ISwPlane>();
                        refPlane.Plane = new Plane(point, normal, dir);
                        refPlane.Commit();
                    }

                    IFeature bboxFeat;

                    if (m_App.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2019))
                    {
                        var bboxFeatData = (BoundingBoxFeatureData)tempPart.Model.FeatureManager.CreateDefinition((int)swFeatureNameID_e.swFmBoundingBox);
                        bboxFeatData.ReferenceFaceOrPlane = (int)fitOpts;
                        bboxFeatData.PlanarEntity = refPlane?.Feature;
                        bboxFeat = tempPart.Model.FeatureManager.CreateFeature(bboxFeatData);

                        if (bboxFeat == null)
                        {
                            throw new Exception("Failed to insert bounding box feature");
                        }
                    }
                    else
                    {
                        if (refPlane != null)
                        {
                            refPlane.Select(false);
                        }

                        bboxFeat = (IFeature)tempPart.Model.FeatureManager.InsertGlobalBoundingBox((int)fitOpts, false, true, out var status);

                        if (bboxFeat == null)
                        {
                            throw new Exception($"Failed to insert bounding box feature. Error code: {(swGlobalBoundingBoxResult_e)status}");
                        }
                    }

                    return ExtractBoxFromBoundingBoxFeature(bboxFeat);
                }
            }
            else
            {
                throw new NotSupportedException("Best fit bounding box can only be calculated in SOLIDWORKS 2018 or newer");
            }
        }

        private Box3D ExtractBoxFromBoundingBoxFeature(IFeature bboxFeat)
        {
            var bboxSketch = (ISketch)bboxFeat.GetSpecificFeature2();

            var lines = ((object[])bboxSketch.GetSketchSegments()).Cast<ISketchLine>().ToArray();
            var points = ((object[])bboxSketch.GetSketchPoints2()).Cast<ISketchPoint>().ToArray();

            var pt1 = points.First();

            ISketchPoint pt2 = null;
            var curDist = double.MinValue;

            var pt1Coord = new Point(pt1.X, pt1.Y, pt1.Z);
            Point pt2Coord = null;

            for (int i = 1; i < points.Length; i++)
            {
                var coord = new Point(points[i].X, points[i].Y, points[i].Z);
                var dist = (pt1Coord - coord).GetLength();

                if (dist > curDist)
                {
                    curDist = dist;
                    pt2 = points[i];
                    pt2Coord = coord;
                }
            }

            var centerPt = new Point((pt1Coord.X + pt2Coord.X) / 2, (pt1Coord.Y + pt2Coord.Y) / 2, (pt1Coord.Z + pt2Coord.Z) / 2);

            var axesLines = lines.Where(l => l.GetStartPoint2() == pt1 || l.GetEndPoint2() == pt1).ToArray();

            if (axesLines.Length == 3)
            {
                Vector GetDirection(ISketchLine line, ISketchPoint orig)
                {
                    var startPt = line.IGetStartPoint2();
                    var endPt = line.IGetEndPoint2();

                    var startCoord = new Point(endPt.X, endPt.Y, endPt.Z);
                    var endCoord = new Point(startPt.X, startPt.Y, startPt.Z);

                    if (orig == startPt)
                    {
                        return endCoord - startCoord;
                    }
                    else if (orig == endPt)
                    {
                        return startCoord - endCoord;
                    }
                    else
                    {
                        throw new Exception("Origin does not belong to a line");
                    }
                }

                var x = GetDirection(axesLines[0], pt1);
                var y = GetDirection(axesLines[1], pt1);
                var z = GetDirection(axesLines[2], pt1);

                double unitConvFactor = 1;

                if (UserUnits)
                {
                    var userUnit = m_Doc.Model.IGetUserUnit((int)swUserUnitsType_e.swLengthUnit);
                    unitConvFactor = userUnit.GetConversionFactor();
                }

                return new Box3D(x.GetLength() * unitConvFactor, y.GetLength() * unitConvFactor, z.GetLength() * unitConvFactor,
                    centerPt.Scale(unitConvFactor),
                    x.Normalize(), y.Normalize(), z.Normalize());
            }
            else
            {
                throw new Exception("Failed to find the axes lines");
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

        public bool BestFit
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
            var bodies = Scope.Select(b => GetTransformedSwBody(b, out _)).ToArray();

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
                        var swBody = GetTransformedSwBody(b, out bool isCopy);

                        if (!isCopy)
                        {
                            swBody = swBody.CreateCopy(m_App);
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
                    if (body is IXWireBody) 
                    {
                        throw new NotSupportedException("Precise bounding box is not supported for the wire bodies");
                    }

                    var swBody = GetTransformedSwBody(body, out _);

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

        private IBody2 GetTransformedSwBody(IXBody srcBody, out bool isCopy)
        {
            var swBody = ((ISwBody)srcBody).Body;

            var comp = (IComponent2)(swBody.IGetFirstFace() as IEntity).GetComponent();

            if (comp != null)
            {
                swBody = swBody.CreateCopy(m_App);

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
        private readonly SwAssembly m_Assm;

        internal SwAssemblyBoundingBox(SwAssembly assm, SwApplication app) : base(assm, app)
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

                if (comps?.Any() != true)
                {
                    return base.Scope;
                }
                else
                {
                    var bodies = comps.SelectMany(c => c.IterateBodies(!VisibleOnly)).ToArray();

                    if (bodies?.Any() != true)
                    {
                        throw new EvaluationFailedException("No bodies found in the component");
                    }

                    return bodies;
                }
            }
            set => base.Scope = value;
        }

        IXComponent[] IAssemblyEvaluation.Scope
        {
            get => m_Creator.CachedProperties.Get<IXComponent[]>(nameof(Scope) + "%Components");
            set
            {
                m_Creator.CachedProperties.Set(value, nameof(Scope) + "%Components");

                if (IsCommitted)
                {
                    m_Creator.Element.Box = CalculateBoundingBox(default);
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
            => ((ISwAssemblyConfiguration)m_Assm.Configurations.Active).Components.SelectMany(c => c.IterateBodies(!VisibleOnly)).ToArray();

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
        private readonly SwPart m_Part;

        internal SwPartBoundingBox(SwPart part, SwApplication app) : base(part, app)
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

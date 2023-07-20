//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Services;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Sketch;
using Xarial.XCad.Toolkit.Utils;
using Xarial.XCad.Utils;

namespace Xarial.XCad.SolidWorks.Features
{
    public interface ISwStructuralMember : IXStructuralMember, ISwFeature
    {
        IStructuralMemberFeatureData StructuralMemberFeatureData { get; }
    }

    public interface ISwSructuralMemberGroupRepository : IXSructuralMemberGroupRepository
    {
    }

    public interface ISwStructuralMemberGroup : IXStructuralMemberGroup
    {
        string Name { get; }
        IStructuralMemberGroup StructuralMemberGroup { get; }
    }

    public interface ISwSructuralMemberPieceRepository : IXSructuralMemberPieceRepository
    {
    }

    public interface ISwSructuralMemberPiece : IXStructuralMemberPiece
    {
    }

    internal class SwSructuralMemberGroupRepository : ISwSructuralMemberGroupRepository
    {
        public IXStructuralMemberGroup this[string name] => RepositoryHelper.Get(this, name);
        
        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters)
            => RepositoryHelper.FilterDefault(this, filters, reverseOrder);

        public bool TryGet(string name, out IXStructuralMemberGroup ent) 
        {
            try
            {
                if (name.StartsWith(SwStructuralMemberGroup.GROUP_BASE_NAME, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (int.TryParse(name.Substring(SwStructuralMemberGroup.GROUP_BASE_NAME.Length), out var groupUserIndex))
                    {
                        var groupIndex = groupUserIndex - 1;

                        var groups = (object[])m_StructMembFeatData.Groups;

                        if (groups.Length > groupIndex)
                        {
                            var group = (IStructuralMemberGroup)groups[groupIndex];

                            ent = new SwStructuralMemberGroup(group, m_Parent, groupIndex);

                            return true;
                        }
                        else
                        {
                            throw new IndexOutOfRangeException($"Weldment contains {groups.Length} groups, target group index is {groupIndex}");
                        }
                    }
                    else 
                    {
                        throw new Exception("Failed to extract group index from the name");
                    }
                }
                else 
                {
                    throw new Exception("Name of the group must start with " + SwStructuralMemberGroup.GROUP_BASE_NAME);
                }
            }
            catch
            {
                ent = null;
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => m_StructMembFeatData.GetGroupsCount();

        private readonly IStructuralMemberFeatureData m_StructMembFeatData;

        private readonly SwStructuralMember m_Parent;

        internal SwSructuralMemberGroupRepository(IStructuralMemberFeatureData structMembFeatData, SwStructuralMember parent) 
        {
            m_StructMembFeatData = structMembFeatData;
            m_Parent = parent;
        }
        
        public void AddRange(IEnumerable<IXStructuralMemberGroup> ents, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public IEnumerator<IXStructuralMemberGroup> GetEnumerator() => EnumerateGroups().GetEnumerator();

        public T PreCreate<T>() where T : IXStructuralMemberGroup
            => throw new NotImplementedException();

        public void RemoveRange(IEnumerable<IXStructuralMemberGroup> ents, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        private IEnumerable<ISwStructuralMemberGroup> EnumerateGroups() 
        {
            int index = 0;

            foreach (IStructuralMemberGroup grp in (object[])m_StructMembFeatData.Groups) 
            {
                yield return new SwStructuralMemberGroup(grp, m_Parent, index++);
            }
        }
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class SwStructuralMemberGroup : ISwStructuralMemberGroup
    {
        internal const string GROUP_BASE_NAME = "Group";

        public void Commit(CancellationToken cancellationToken) => throw new NotImplementedException();
        public bool IsCommitted => true;

        public IStructuralMemberGroup StructuralMemberGroup { get; }

        internal SwStructuralMember Parent { get; }

        internal int Index { get; }

        public SwStructuralMemberGroup(IStructuralMemberGroup grp, SwStructuralMember parent, int index) 
        {
            StructuralMemberGroup = grp;
            Parent = parent;

            Index = index;

            Pieces = new SwSructuralMemberPieceRepository(this);
        }

        public IXSructuralMemberPieceRepository Pieces { get; }

        public string Name => GROUP_BASE_NAME + (Index + 1).ToString();
    }

    internal class SwSructuralMemberPieceRepository : ISwSructuralMemberPieceRepository
    {
        public IXStructuralMemberPiece this[string name] => RepositoryHelper.Get(this, name);
        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) => RepositoryHelper.FilterDefault(this, filters, reverseOrder);

        public bool TryGet(string name, out IXStructuralMemberPiece ent) 
        {
            var piece = EnumeratePieces().FirstOrDefault(p => string.Equals(p.Body.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (piece != null)
            {
                ent = piece;
                return true;
            }
            else 
            {
                ent = null;
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => m_Group.StructuralMemberGroup.GetSegmentsCount();

        private readonly SwStructuralMemberGroup m_Group;

        internal SwSructuralMemberPieceRepository(SwStructuralMemberGroup group)
        {
            m_Group = group;
        }

        public void AddRange(IEnumerable<IXStructuralMemberPiece> ents, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public IEnumerator<IXStructuralMemberPiece> GetEnumerator() => EnumeratePieces().GetEnumerator();

        public T PreCreate<T>() where T : IXStructuralMemberPiece
            => throw new NotImplementedException();

        public void RemoveRange(IEnumerable<IXStructuralMemberPiece> ents, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        private IEnumerable<IXStructuralMemberPiece> EnumeratePieces()
        {
            var structMembFeat = m_Group.Parent;
            var structMembFeatData = structMembFeat.StructuralMemberFeatureData;
            
            var grpSegments = ((object[])m_Group.StructuralMemberGroup.Segments).Cast<ISketchSegment>().ToArray();

            foreach (var body in m_Group.Parent.IterateBodies().OfType<SwSolidBody>())
            {
                ISketchSegment[] segments;

                if (structMembFeat.OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2015))
                {
                    segments = ((object[])structMembFeatData.GetSketchSegments((Body2)body.Body)).Cast<ISketchSegment>().ToArray();
                }
                else 
                {
                    segments = new ISketchSegment[] { structMembFeatData.GetPathSegmentAt((Body2)body.Body) };
                }

                //segments belong to this group
                if (!segments.Except(grpSegments).Any()) 
                {
                    yield return new SwStructuralMemberPiece(body, 
                        segments.Select(s=>structMembFeat.OwnerDocument.CreateObjectFromDispatch<ISwSketchSegment>(s)).ToArray(), m_Group);
                }
            }
        }
    }

    [DebuggerDisplay("{" + nameof(Body) + "." + nameof(IXBody.Name) + "}")]
    internal class SwStructuralMemberPiece : ISwSructuralMemberPiece
    {
        public IXSolidBody Body => m_Body;

        public IXSketchSegment[] Segments => m_Segments;

        public Plane ProfilePlane
        {
            get
            {
                var group = m_Group.StructuralMemberGroup;

                var segsArr = ((object[])group.Segments)
                    .Cast<ISketchSegment>()
                    .Select(s => m_Group.Parent.OwnerDocument.CreateObjectFromDispatch<ISwSketchSegment>(s))
                    .ToArray();

                if (m_Group.Index == 0)
                {
                    ValidateGroup(group, m_Group.Parent.Profile, segsArr);
                }

                var alignment = GetAlignment(group, out var alignAxis);

                var transform = GetInitialProfileLocation((ISwSketchLine)segsArr[0], alignment, alignAxis,
                    group.Angle, group.MirrorProfile ? (swMirrorProfileOrAlignmentAxis_e?)group.MirrorProfileAxis : null);

                var segIndices = m_Segments.Select(s => Array.FindIndex(segsArr, x => x.Equals(s))).ToArray();

                for (int i = 0; i < segIndices[0]; i++)
                {
                    var prevSeg = segsArr[i];
                    var thisSeg = segsArr[i + 1];

                    if (prevSeg is ISwSketchLine && thisSeg is ISwSketchLine)
                    {
                        var prevLine = (ISwSketchLine)prevSeg;
                        var thisLine = (ISwSketchLine)thisSeg;

                        IXSketchPoint startPt;
                        IXSketchPoint midPt;
                        IXSketchPoint endPt;

                        if (prevLine.StartPoint.Equals(thisLine.EndPoint))
                        {
                            startPt = prevLine.EndPoint;
                            midPt = prevLine.StartPoint;
                            endPt = thisLine.StartPoint;
                        }
                        else if (prevLine.StartPoint.Equals(thisLine.StartPoint))
                        {
                            startPt = prevLine.EndPoint;
                            midPt = prevLine.StartPoint;
                            endPt = thisLine.EndPoint;
                        }
                        else if (prevLine.EndPoint.Equals(thisLine.EndPoint))
                        {
                            startPt = prevLine.StartPoint;
                            midPt = prevLine.EndPoint;
                            endPt = thisLine.StartPoint;
                        }
                        else if (prevLine.EndPoint.Equals(thisLine.StartPoint))
                        {
                            startPt = prevLine.StartPoint;
                            midPt = prevLine.EndPoint;
                            endPt = thisLine.EndPoint;
                        }
                        else
                        {
                            if (GetDirection(prevLine).IsParallel(GetDirection(thisLine)))
                            {
                                //parallel disconnected segments have the same orientation
                                var origin = new Point(0, 0, 0).Transform(transform);
                                transform *= TransformMatrix.CreateFromTranslation(origin - GetCoordinateInGlobalSpace(thisLine.StartPoint));
                                continue;
                            }
                            else
                            {
                                throw new Exception("Segments are not connected and not parallel");
                            }
                        }

                        var midCoord = GetCoordinateInGlobalSpace(midPt);

                        var prevDir = GetCoordinateInGlobalSpace(startPt) - midCoord;
                        var thisDir = midCoord - GetCoordinateInGlobalSpace(endPt);

                        var ang = prevDir.GetAngle(thisDir);

                        var axis = prevDir.Cross(thisDir);

                        transform = transform
                            .Multiply(TransformMatrix.CreateFromTranslation(prevDir))
                            .Multiply(TransformMatrix.CreateFromRotationAroundAxis(axis, ang, midCoord));
                    }
                    else
                    {
                        throw new NotSupportedException("Only linear sketch segments are supported");
                    }
                }

                return new Plane(
                    new Point(0, 0, 0).Transform(transform),
                    new Vector(0, 0, 1).Transform(transform),
                    new Vector(1, 0, 0).Transform(transform));
            }
        }

        private void ValidateGroup(IStructuralMemberGroup group, IXSketch2D profileSketch, ISwSketchSegment[] segsArr)
        {
            if (group.MergeArcSegmentBodies && segsArr.Any(s => s is IXSketchArc))
            {
                throw new NotSupportedException("Merge Arc Segment Bodies option is not supported");
            }

            if (group.MiterMergeCondition && segsArr.Length > 1)
            {
                throw new NotSupportedException("Merge Miter Bodies option is not supported");
            }

            var profilePlane = profileSketch.Plane;

            var firstSeg = segsArr.First();

            if (!(firstSeg is IXSketchLine)) 
            {
                throw new NotSupportedException("First segment must be a sketch line");
            }

            if (!Numeric.Compare(profilePlane.GetDistance(GetCoordinateInGlobalSpace(((IXSketchLine)firstSeg).StartPoint)), 0)
                && !Numeric.Compare(profilePlane.GetDistance(GetCoordinateInGlobalSpace(((IXSketchLine)firstSeg).EndPoint)), 0)) 
            {
                throw new NotSupportedException("Profile sketch must be located in the first segment of the structural member");
            }
        }

        private Vector GetAlignment(IStructuralMemberGroup grp, out swMirrorProfileOrAlignmentAxis_e? alignAxis) 
        {
            var alignmentVectEnt = grp.AlignmentVector;

            if (alignmentVectEnt != null)
            {
                var alignmentEnt = m_Body.OwnerDocument.CreateObjectFromDispatch<ISwObject>(alignmentVectEnt);

                Vector alignment;

                switch (alignmentEnt) 
                {
                    case ISwLinearEdge edge:
                        alignment = GetDirection(edge.Definition);
                        break;

                    case IXSketchLine line:
                        alignment = GetDirection(line);
                        
                        var sketch = line.OwnerSketch;
                        
                        if (sketch is IXSketch2D) 
                        {
                            alignment *= ((IXSketch2D)sketch).Plane.GetTransformation();
                        }
                        break;

                    default:
                        throw new NotSupportedException("Only linear edges and sketch lines are supported as alignment entities");
                }

                alignAxis = (swMirrorProfileOrAlignmentAxis_e)grp.AlignAxis;

                return alignment;
            }
            else 
            {
                alignAxis = null;
                return null;
            }
        }

        private TransformMatrix GetInitialProfileLocation(ISwSketchLine line, Vector alignment,
            swMirrorProfileOrAlignmentAxis_e? alignAxis,
            double profileAngle, swMirrorProfileOrAlignmentAxis_e? mirror)
        {
            var ownerSketch = line.OwnerSketch;

            var baselineZ = new Vector(0, 0, 1);

            var dir = GetDirection(line);

            var startCoord = line.StartPoint.Coordinate;

            if (ownerSketch is ISwSketch2D)
            {
                var sketch = (ISwSketch2D)ownerSketch;
                
                var sketchTransform = sketch.Plane.GetTransformation();

                dir *= sketchTransform;
                startCoord *= sketchTransform;

                if (alignment == null) 
                {
                    //Profiles for the segments on 2D sketch are automatically aligned with the sketch coordinate system
                    alignment = new Vector(-1, 0, 0) * sketchTransform;

                    if (alignment.IsParallel(dir)) 
                    {
                        alignment = new Vector(0, -1, 0) * sketchTransform;
                    }

                    alignAxis = swMirrorProfileOrAlignmentAxis_e.swMirrorProfileOrAlignmentAxis_Horizontal;
                }
            }

            var angle = baselineZ.GetAngle(dir);
            var rotVect = baselineZ.Cross(dir);

            //finding the transform to align the direction of the line to be a z-axis
            var transform = TransformMatrix.CreateFromRotationAroundAxis(rotVect, angle, new Point(0, 0, 0))
                    * TransformMatrix.CreateFromTranslation(startCoord.ToVector());

            if (alignment != null) 
            {
                if (dir.IsParallel(alignment)) 
                {
                    throw new Exception("Alignment cannot be parallel to direction segment");
                }

                var curAlignmentVec = new Vector(1, 0, 0) * transform;

                var alignmentPlane = new Plane(startCoord, dir, curAlignmentVec);

                var alignmentAngle = curAlignmentVec.GetAngleOnPlane(alignment, alignmentPlane);

                switch (alignAxis) 
                {
                    case swMirrorProfileOrAlignmentAxis_e.swMirrorProfileOrAlignmentAxis_Horizontal:
                        //do nothing
                        break;

                    case swMirrorProfileOrAlignmentAxis_e.swMirrorProfileOrAlignmentAxis_Vertical:
                        alignmentAngle -= Math.PI / 2;
                        break;

                    default:
                        throw new NotSupportedException();
                }

                //aligning X axis of the profile
                transform *= TransformMatrix.CreateFromRotationAroundAxis(dir, alignmentAngle, startCoord);
            }

            transform *= TransformMatrix.CreateFromRotationAroundAxis(dir, profileAngle, startCoord);

            if (mirror.HasValue) 
            {
                Vector mirrorAxis;

                switch (mirror) 
                {
                    case swMirrorProfileOrAlignmentAxis_e.swMirrorProfileOrAlignmentAxis_Horizontal:
                        mirrorAxis = new Vector(1, 0, 0) * transform;
                        break;

                    case swMirrorProfileOrAlignmentAxis_e.swMirrorProfileOrAlignmentAxis_Vertical:
                        mirrorAxis = new Vector(0, 1, 0) * transform;
                        break;

                    default:
                        throw new NotSupportedException();
                }

                transform *= TransformMatrix.CreateFromRotationAroundAxis(mirrorAxis, Math.PI, startCoord);
            }

            return transform;
        }

        private Point GetCoordinateInGlobalSpace(IXSketchPoint point) 
        {
            var coord = point.Coordinate;

            var sketch = point.OwnerSketch;

            if (sketch is IXSketch2D)
            {
                return coord.Transform(((IXSketch2D)sketch).Plane.GetTransformation());
            }
            else 
            {
                return coord;
            }
        }

        private Vector GetDirection(IXLine line)
            => line.StartPoint.Coordinate - line.EndPoint.Coordinate;

        public bool IsCommitted => true;
        public void Commit(CancellationToken cancellationToken) => throw new NotImplementedException();
        
        private readonly SwStructuralMemberGroup m_Group;

        private readonly ISwSketchSegment[] m_Segments;

        private readonly SwSolidBody m_Body;

        public SwStructuralMemberPiece(SwSolidBody body, ISwSketchSegment[] segments, SwStructuralMemberGroup group)
        {
            m_Body = body;
            m_Segments = segments;
            m_Group = group;
        }
    }

    internal class SwStructuralMember : SwFeature, ISwStructuralMember
    {
        public IStructuralMemberFeatureData StructuralMemberFeatureData { get; }

        internal SwStructuralMember(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(feat, doc, app, created)
        {
            StructuralMemberFeatureData = (IStructuralMemberFeatureData)feat.GetDefinition();

            Groups = new SwSructuralMemberGroupRepository(StructuralMemberFeatureData, this);
        }

        public IXSketch2D Profile 
        {
            get 
            {
                var profileSketch = FeatureEnumerator.IterateSubFeatures(Feature, false)
                    .FirstOrDefault(f => f.GetTypeName2() == SwSketch2D.TypeName);

                if (profileSketch != null)
                {
                    return OwnerDocument.CreateObjectFromDispatch<ISwSketch2D>(profileSketch);
                }
                else 
                {
                    throw new Exception("Failed to find the profile sketch");
                }
            }
        }

        public IXSructuralMemberGroupRepository Groups { get; }
    }
}

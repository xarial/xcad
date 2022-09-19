//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Sketch;
using Xarial.XCad.Toolkit.Utils;

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
            var grp = EnumerateGroups().FirstOrDefault(g => string.Equals(g.Name, name));

            if (grp != null)
            {
                ent = grp;
                return true;
            }
            else 
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

        public string Name => $"Group{Index + 1}";
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

            foreach (var body in m_Group.Parent.IterateBodies().OfType<ISwSolidBody>())
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
        public IXSolidBody Body { get; }

        public IXSketchSegment[] Segments => m_Segments;

        public Plane ProfilePlane
        {
            get 
            {
                if (m_Group.Index == 0)
                {
                    var profilePlane = m_Group.Parent.Profile.Plane;

                    var segsArr = ((object[])m_Group.StructuralMemberGroup.Segments).Cast<ISketchSegment>().ToArray();

                    var segIndices = m_Segments.Select(s => Array.IndexOf(segsArr, s.Segment)).ToArray();

                    if (segIndices[0] == 0)
                    {
                        return profilePlane;
                    }
                    else 
                    {
                        if (m_Group.StructuralMemberGroup.MergeArcSegmentBodies) 
                        {
                            throw new NotSupportedException("Merge Arc Segment Bodies option is not supported");
                        }

                        var transform = profilePlane.GetTransformation();

                        for (int i = 0; i < segIndices[0]; i++) 
                        {
                            var prevSeg = segsArr[i];
                            var thisSeg = segsArr[i + 1];

                            if (prevSeg is ISketchLine && thisSeg is ISketchLine)
                            {
                                var prevLine = m_Group.Parent.OwnerDocument.CreateObjectFromDispatch<ISwSketchLine>(prevSeg);
                                var thisLine = m_Group.Parent.OwnerDocument.CreateObjectFromDispatch<ISwSketchLine>(thisSeg);

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
                                    throw new Exception("Segments are not connected");
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
                else 
                {
                    throw new NotSupportedException("Only single group is supported");
                }
            }
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

        public bool IsCommitted => true;
        public void Commit(CancellationToken cancellationToken) => throw new NotImplementedException();
        
        private readonly SwStructuralMemberGroup m_Group;

        private readonly ISwSketchSegment[] m_Segments;

        public SwStructuralMemberPiece(IXSolidBody body, ISwSketchSegment[] segments, SwStructuralMemberGroup group)
        {
            Body = body;
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

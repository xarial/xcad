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
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Evaluation;
using Xarial.XCad.Geometry.Exceptions;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.Toolkit.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Evaluation
{
    /// <summary>
    /// Tesselation data
    /// </summary>
    public class BodyTesselation
    {
        /// <summary>
        /// Source body
        /// </summary>
        public IXBody Body { get; }

        /// <summary>
        /// Tesselation obect
        /// </summary>
        public ITessellation Tesselation { get; }

        /// <summary>
        /// Tesselation transformation
        /// </summary>
        public TransformMatrix Transform { get; }

        internal BodyTesselation(IXBody body, ITessellation tesselation, TransformMatrix transform)
        {
            Body = body;
            Tesselation = tesselation;
            Transform = transform;
        }
    }

    /// <summary>
    /// Represents SOLIDWORKS specific <see cref="IXTessellation"/>
    /// </summary>
    public interface ISwTessellation : IXTessellation
    {
        /// <summary>
        /// Tesselation of each body in the scope
        /// </summary>
        IReadOnlyList<BodyTesselation> Tessellation { get; }
    }

    /// <summary>
    /// Represents <see cref="IXAssembly"/> specific <see cref="IXTessellation"/>
    /// </summary>
    public interface ISwAssemblyTessellation : ISwTessellation, IXAssemblyTessellation
    {
    }

    internal abstract class SwTessellation : ISwTessellation
    {
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

        public bool IsCommitted => m_Creator.IsCreated;

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

        public IReadOnlyList<BodyTesselation> Tessellation => m_Creator.Element;

        public IEnumerable<Point> Positions 
        {
            get 
            {
                double lengthConvFactor;

                if (UserUnits) 
                {
                    lengthConvFactor = m_Doc.Units.GetLengthConversionFactor();
                }
                else 
                {
                    lengthConvFactor = 1;
                }

                foreach (var tessData in Tessellation)
                {
                    var tess = tessData.Tesselation;
                    var transform = GetTransformation(tessData);

                    for (int i = 0; i < tess.GetVertexCount(); i++)
                    {
                        var pos = new Point((double[])tess.GetVertexPoint(i));

                        if (transform != null) 
                        {
                            pos *= transform;
                        }

                        if (UserUnits)
                        {
                            pos = pos.Scale(lengthConvFactor);
                        }

                        yield return pos;
                    }
                }
            }
        }

        public IEnumerable<int> TriangleIndices 
        {
            get
            {
                var offset = 0;

                foreach (var tessData in Tessellation)
                {
                    var tess = tessData.Tesselation;

                    for (int i = 0; i < tess.GetFacetCount(); i++)
                    {
                        var fins = (int[])tess.GetFacetFins(i);

                        if (fins.Length == 3)
                        {
                            var vertices = (int[])tess.GetFinVertices(fins[0]);

                            yield return vertices[0] + offset;
                            yield return vertices[1] + offset;

                            if (vertices.Length == 2)
                            {
                                var nextVertices = (int[])tess.GetFinVertices(fins[1]);

                                if (nextVertices.Length == 2)
                                {
                                    if (nextVertices[0] == vertices[0] || nextVertices[0] == vertices[1])
                                    {
                                        yield return nextVertices[1] + offset;
                                    }
                                    else if (nextVertices[1] == vertices[0] || nextVertices[1] == vertices[1])
                                    {
                                        yield return nextVertices[0] + offset;
                                    }
                                    else
                                    {
                                        throw new Exception("Fin vertices are not connected");
                                    }
                                }
                                else
                                {
                                    throw new Exception("It must be 2 vertices per fin");
                                }
                            }
                            else
                            {
                                throw new Exception("It must be 2 vertices per fin");
                            }
                        }
                        else 
                        {
                            throw new Exception("It must be 3 fins per facet");
                        }
                    }

                    offset += tess.GetVertexCount();
                }
            }
        }

        public IEnumerable<Vector> Normals
        {
            get
            {
                foreach (var tessData in Tessellation)
                {
                    var tess = tessData.Tesselation;

                    var transform = GetTransformation(tessData);

                    for (int i = 0; i < tess.GetVertexCount(); i++)
                    {
                        var norm = new Vector((double[])tess.GetVertexNormal(i));

                        if (transform != null) 
                        {
                            norm *= transform;
                        }

                        yield return norm;
                    }
                }
            }
        }

        public void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);

        private IReadOnlyList<BodyTesselation> CreateTesselation(CancellationToken cancellationToken)
        {
            var bodies = Scope?.Select(b => new Tuple<IXBody, IXFace[]>(b, null)).ToArray();

            if (bodies?.Any() != true)
            {
                bodies = GetAllBodies();
            }

            var res = new List<BodyTesselation>();

            foreach (var bodyData in bodies)
            {
                var body = (ISwBody)bodyData.Item1;
                var faces = bodyData.Item2?.Cast<ISwFace>().Select(f => f.Face).ToArray();

                var comp = body.Component;

                TransformMatrix transform;

                //assembly bodies must be transformed to the assembly space
                if (comp != null)
                {
                    transform = comp.Transformation;
                }
                else
                {
                    transform = null;
                }

                var tess = (ITessellation)body.Body.GetTessellation(faces);
                
                tess.NeedVertexNormal = true;
                tess.ImprovedQuality = Precise;

                if (!tess.Tessellate())
                {
                    throw new Exception($"Failed to tesselate body: '{body.Name}'");
                }

                res.Add(new BodyTesselation(body, tess, transform));
            }

            return res;
        }

        private TransformMatrix GetTransformation(BodyTesselation tessData)
        {
            var transform = tessData.Transform;

            var relTo = RelativeTo;

            if (transform != null)
            {
                if (relTo != null)
                {
                    transform *= relTo;
                }
            }
            else
            {
                transform = relTo;
            }

            return transform;
        }

        protected readonly IElementCreator<IReadOnlyList<BodyTesselation>> m_Creator;

        private readonly ISwDocument3D m_Doc;

        internal SwTessellation(ISwDocument3D doc)
        {
            m_Doc = doc;

            m_Creator = new ElementCreator<IReadOnlyList<BodyTesselation>>(CreateTesselation, null, false);
        }

        protected abstract Tuple<IXBody, IXFace[]>[] GetAllBodies();

        public void Dispose()
        {
        }
    }

    internal class SwPartTesselation : SwTessellation
    {
        private readonly ISwPart m_Part;

        internal SwPartTesselation(ISwPart part) : base(part)
        {
            m_Part = part;
        }

        protected override Tuple<IXBody, IXFace[]>[] GetAllBodies()
            => m_Part.Bodies.OfType<IXSolidBody>()
            .Where(b => !VisibleOnly || b.Visible)
            .Select(b => new Tuple<IXBody, IXFace[]>(b, null)).ToArray();
    }

    internal class SwAssemblyTesselation : SwTessellation, ISwAssemblyTessellation
    {
        private readonly ISwAssembly m_Assm;

        internal SwAssemblyTesselation(ISwAssembly assm) : base(assm)
        {
            m_Assm = assm;
        }

        public override IXBody[] Scope
        {
            get
            {
                var comps = (this as IXAssemblyTessellation).Scope;

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
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value, nameof(Scope) + "%Components");
                }
                else
                {
                    throw new CommittedElementPropertyChangeNotSupported();
                }
            }
        }

        protected override Tuple<IXBody, IXFace[]>[] GetAllBodies()
            => m_Assm.Configurations.Active.Components.SelectMany(c => c.IterateBodies(!VisibleOnly))
            .Select(b => new Tuple<IXBody, IXFace[]>(b, null)).ToArray();
    }

    internal class SwFaceTesselation : SwTessellation, IXFaceTesselation
    {
        //public new IXFace[] Scope
        //{
        //    get => m_Creator.CachedProperties.Get<IXFace[]>();
        //    set
        //    {
        //        if (!IsCommitted)
        //        {
        //            m_Creator.CachedProperties.Set(value);
        //        }
        //        else
        //        {
        //            throw new CommittedElementPropertyChangeNotSupported();
        //        }
        //    }
        //}

        IXFace[] IXFaceTesselation.Scope
        {
            get => m_Creator.CachedProperties.Get<IXFace[]>(nameof(Scope) + "%Faces");
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value, nameof(Scope) + "%Faces");
                }
                else
                {
                    throw new CommittedElementPropertyChangeNotSupported();
                }
            }
        }

        public SwFaceTesselation(SwDocument3D doc) : base(doc)
        {
        }

        protected override Tuple<IXBody, IXFace[]>[] GetAllBodies()
        {
            var faces = ((IXFaceTesselation)this).Scope;

            if (faces?.Any() == true)
            {
                return faces.GroupBy(s => s.Body, new XObjectEqualityComparer<IXBody>())
                    .Select(g => new Tuple<IXBody, IXFace[]>(g.Key, g.ToArray())).ToArray();
            }
            else
            {
                throw new Exception("No faces are specified in the scope");
            }
        }
    }
}

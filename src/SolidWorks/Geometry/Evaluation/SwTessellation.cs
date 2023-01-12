//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
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
    public interface ISwTessellation : IXTessellation
    {
        IReadOnlyDictionary<IXBody, ITessellation> Tessellation { get; }
    }

    public interface ISwAssemblyTessellation : ISwTessellation, IXAssemblyTessellation
    {
    }

    internal abstract class SwTessellation : ISwTessellation
    {
        //TODO: implement relative to matrix
        public TransformMatrix RelativeTo { get => TransformMatrix.Identity; set => throw new NotImplementedException(); }

        //TODO: implement handling of user units
        public bool UserUnits { get => false; set => throw new NotImplementedException(); }

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

        public IEnumerable<TesselationTriangle> Triangles
        {
            get
            {
                foreach (var tess in Tessellation.Values)
                {
                    for (int i = 0; i < tess.GetFacetCount(); i++)
                    {
                        var fins = (int[])tess.GetFacetFins(i);

                        var vertices = fins.SelectMany(f => (int[])tess.GetFinVertices(f)).Distinct().ToArray();

                        yield return new TesselationTriangle(
                            new Vector((double[])tess.GetVertexNormal(vertices[0])),
                            new Vector((double[])tess.GetVertexPoint(vertices[0])),
                            new Vector((double[])tess.GetVertexPoint(vertices[1])),
                            new Vector((double[])tess.GetVertexPoint(vertices[2])));
                    }
                }
            }
        }

        public IReadOnlyDictionary<IXBody, ITessellation> Tessellation => m_Creator.Element;

        public void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);

        private IReadOnlyDictionary<IXBody, ITessellation> CreateTesselation(CancellationToken cancellationToken)
        {
            var bodies = Scope;

            if (bodies?.Any() != true)
            {
                bodies = GetAllBodies();
            }

            var res = new Dictionary<IXBody, ITessellation>();

            foreach (ISwBody body in bodies)
            {
                ISwBody tessBody;

                var comp = body.Component;

                //assembly bodies must be transformed to the assembly space
                if (comp != null)
                {
                    var copy = body.Copy();
                    copy.Transform(comp.Transformation);
                    tessBody = (ISwBody)copy;
                }
                else
                {
                    tessBody = body;
                }

                var tess = (ITessellation)tessBody.Body.GetTessellation(null);

                tess.NeedVertexNormal = true;
                tess.ImprovedQuality = Precise;

                if (!tess.Tessellate())
                {
                    throw new Exception($"Failed to tesselate body: '{body.Name}'");
                }

                res.Add(body, tess);
            }

            return res;
        }

        protected readonly IElementCreator<IReadOnlyDictionary<IXBody, ITessellation>> m_Creator;

        private readonly ISwDocument3D m_Doc;

        internal SwTessellation(ISwDocument3D doc)
        {
            m_Doc = doc;

            m_Creator = new ElementCreator<IReadOnlyDictionary<IXBody, ITessellation>>(CreateTesselation, null, false);
        }

        protected abstract IXBody[] GetAllBodies();

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

        protected override IXBody[] GetAllBodies()
            => m_Part.Bodies.OfType<IXSolidBody>()
            .Where(b => !VisibleOnly || b.Visible).ToArray();
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
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value, nameof(Scope) + "_Components");
                }
                else
                {
                    throw new CommittedElementPropertyChangeNotSupported();
                }
            }
        }

        protected override IXBody[] GetAllBodies()
            => m_Assm.Configurations.Active.Components.SelectMany(c => c.IterateBodies(!VisibleOnly)).ToArray();
    }
}

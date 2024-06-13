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
    /// Represents SOLIDWORKS specific <see cref="IXTessellation"/>
    /// </summary>
    public interface ISwTessellation : IXTessellation
    {
        /// <summary>
        /// Tesselation of each body in the scope
        /// </summary>
        IReadOnlyDictionary<IXBody, ITessellation> Tessellation { get; }
    }

    /// <summary>
    /// Represents <see cref="IXAssembly"/> specific <see cref="IXTessellation"/>
    /// </summary>
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

        protected override IXBody[] GetAllBodies()
            => m_Assm.Configurations.Active.Components.SelectMany(c => c.IterateBodies(!VisibleOnly)).ToArray();
    }

    internal class SwFaceTesselation : IXFaceTesselation
    {
        private class ImageQualitySetter : IDisposable 
        {
            private const int MAX_IMAGE_QUALITY_WIREFRAME_VALUE = 100;

            private IModelDocExtension m_DocExt;

            private double? m_ImageQualityShadedDeviation;
            private int? m_ImageQualityWireframeValue;

            internal ImageQualitySetter(SwDocument doc, bool precise) 
            {
                m_DocExt = doc.Model.Extension;

                if (precise) 
                {
                    var curVal = double.NaN;
                    var minVal = double.NaN;
                    var maxVal = double.NaN;

                    m_DocExt.GetUserPreferenceDoubleValueRange((int)swUserPreferenceDoubleValue_e.swImageQualityShadedDeviation,
                        ref curVal, ref minVal, ref maxVal);

                    m_ImageQualityShadedDeviation = m_DocExt.GetUserPreferenceDouble(
                        (int)swUserPreferenceDoubleValue_e.swImageQualityShadedDeviation,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);

                    m_ImageQualityWireframeValue = m_DocExt.GetUserPreferenceInteger(
                        (int)swUserPreferenceIntegerValue_e.swImageQualityWireframeValue, 
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);

                    m_DocExt.SetUserPreferenceDouble((int)swUserPreferenceDoubleValue_e.swImageQualityShadedDeviation, 
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, minVal);

                    m_DocExt.SetUserPreferenceInteger(
                        (int)swUserPreferenceIntegerValue_e.swImageQualityWireframeValue,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, MAX_IMAGE_QUALITY_WIREFRAME_VALUE);
                }
            }

            public void Dispose()
            {
                if (m_ImageQualityShadedDeviation.HasValue)
                {
                    m_DocExt.SetUserPreferenceDouble((int)swUserPreferenceDoubleValue_e.swImageQualityShadedDeviation,
                            (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, m_ImageQualityShadedDeviation.Value);
                }

                if (m_ImageQualityWireframeValue.HasValue)
                {
                    m_DocExt.SetUserPreferenceInteger(
                        (int)swUserPreferenceIntegerValue_e.swImageQualityWireframeValue,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, m_ImageQualityWireframeValue.Value);
                }
            }
        }

        IXBody[] IEvaluation.Scope { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public IXFace[] Scope
        {
            get => m_Creator.CachedProperties.Get<IXFace[]>();
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

        //TODO: implement relative to matrix
        public TransformMatrix RelativeTo { get => TransformMatrix.Identity; set => throw new NotImplementedException(); }
        
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

        public bool VisibleOnly { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        
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

        public bool IsCommitted => m_Creator.IsCreated;

        public IEnumerable<TesselationTriangle> Triangles => m_Creator.Element;

        private readonly IElementCreator<TesselationTriangle[]> m_Creator;

        private readonly SwDocument3D m_Doc;

        public SwFaceTesselation(SwDocument3D doc) 
        {
            m_Doc = doc;
            m_Creator = new ElementCreator<TesselationTriangle[]>(CreateTesselation, null, false);
        }

        private TesselationTriangle[] CreateTesselation(CancellationToken cancellationToken)
        {
            if (Scope?.Any() == true)
            {
                //TODO: consider components transformation

                using (new ImageQualitySetter(m_Doc, Precise))
                {
                    var triangs = new List<TesselationTriangle>();

                    foreach (ISwFace face in Scope)
                    {
                        TransformMatrix transform;

                        var comp = face.Component;

                        if (comp != null)
                        {
                            transform = comp.Transformation;
                        }
                        else 
                        {
                            transform = TransformMatrix.Identity;
                        }

                        var tessTriangs = (float[])face.Face.GetTessTriangles(!UserUnits);
                        var tessNorms = (float[])face.Face.GetTessNorms();

                        if (tessTriangs.Length == tessNorms.Length)
                        {
                            for (int i = 0; i < tessTriangs.Length; i += 9)
                            {
                                triangs.Add(new TesselationTriangle(
                                    new Vector(tessNorms[i], tessNorms[i + 1], tessNorms[i + 2]) * transform,
                                    new Point(tessTriangs[i], tessTriangs[i + 1], tessTriangs[i + 2]) * transform,
                                    new Point(tessTriangs[i + 3], tessTriangs[i + 4], tessTriangs[i + 5]) * transform,
                                    new Point(tessTriangs[i + 6], tessTriangs[i + 7], tessTriangs[i + 8]) * transform));
                            }
                        }
                        else
                        {
                            throw new Exception("Size of triangles mismatch");
                        }
                    }

                    return triangs.ToArray();
                }
            }
            else 
            {
                throw new Exception("No faces are specified in the scope");
            }
        }

        public void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        public void Dispose()
        {
        }
    }
}

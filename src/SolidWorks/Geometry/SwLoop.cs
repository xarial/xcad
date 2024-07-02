//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Sketch;

namespace Xarial.XCad.SolidWorks.Geometry
{
    /// <summary>
    /// SOLIDWORKS-specific loop
    /// </summary>
    public interface ISwLoop : IXLoop, ISwSelObject
    {
        /// <summary>
        /// Pointer to SOLIDWORKS loop
        /// </summary>
        ILoop2 Loop { get; }
    }

    internal class SwLoop : SwSelObject, ISwLoop
    {
        public ILoop2 Loop 
        {
            get 
            {
                if (IsCommitted)
                {
                    if (!m_IsVirtual)
                    {
                        return m_Creator.Element;
                    }
                    else 
                    {
                        throw new Exception("Virtual loop cannot be accessed");
                    }
                }
                else 
                {
                    throw new NonCommittedElementAccessException();
                }
            }
        }

        public override bool IsCommitted => m_Creator.IsCreated;
                
        public IXSegment[] Segments
        {
            get
            {
                if (IsCommitted)
                {
                    if (!m_IsVirtual)
                    {
                        return (Loop.GetEdges() as object[])
                            .Cast<IEdge>()
                            .Select(e => OwnerApplication.CreateObjectFromDispatch<ISwEdge>(e, OwnerDocument))
                            .ToArray();
                    }
                    else
                    {
                        return m_VirtualSegments;
                    }
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<IXSegment[]>();
                }
            }
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        private readonly ElementCreator<ILoop2> m_Creator;

        private bool m_IsVirtual;
        private IXSegment[] m_VirtualSegments;

        internal SwLoop(ILoop2 loop, SwDocument doc, SwApplication app) : base(loop, doc, app)
        {
            //new loops cannot be create, so this loop is just a placeholder of curves for other methods which requires loops
            m_IsVirtual = loop == null;
            m_Creator = new ElementCreator<ILoop2>(CreateLoop, loop, loop != null);
        }

        private ILoop2 CreateLoop(CancellationToken cancellationToken) 
        {
            if (m_IsVirtual)
            {
                m_VirtualSegments = m_Creator.CachedProperties.Get<IXSegment[]>(nameof(Segments));
                return null;
            }
            else 
            {
                throw new NotSupportedException();
            }
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);
    }

    internal static class SwLoopExtension 
    {
        internal static IEnumerable<ISwCurve> IterateCurves(this ISwLoop loop)
        {
            foreach (var seg in loop.Segments)
            {
                ISwCurve segCurve;

                switch (seg)
                {
                    case ISwCurve curve:
                        segCurve = curve;
                        break;

                    case ISwEdge edge:
                        segCurve = edge.Definition;
                        break;

                    case ISwSketchSegment skSeg:
                        segCurve = skSeg.Definition;
                        break;

                    default:
                        throw new NotSupportedException();
                }

                yield return segCurve;
            }
        }
    }
}

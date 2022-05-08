//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Primitives
{
    public interface ISwTempExtrusion : IXExtrusion, ISwTempPrimitive
    {
        new ISwRegion[] Profiles { get; set; }
    }

    internal class SwTempExtrusion : SwTempPrimitive, ISwTempExtrusion
    {
        IXRegion[] IXExtrusion.Profiles
        {
            get => Profiles;
            set => Profiles = value?.Cast<ISwRegion>().ToArray();
        }

        public double Depth
        {
            get => m_Creator.CachedProperties.Get<double>();
            set
            {
                if (IsCommitted)
                {
                    throw new CommitedSegmentReadOnlyParameterException();
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public Vector Direction
        {
            get => m_Creator.CachedProperties.Get<Vector>();
            set
            {
                if (IsCommitted)
                {
                    throw new CommitedSegmentReadOnlyParameterException();
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public ISwRegion[] Profiles
        {
            get => m_Creator.CachedProperties.Get<ISwRegion[]>();
            set
            {
                if (IsCommitted)
                {
                    throw new CommitedSegmentReadOnlyParameterException();
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        internal SwTempExtrusion(SwTempBody[] bodies, ISwApplication app, bool isCreated) 
            : base(bodies, app, isCreated)
        {
        }

        protected override ISwTempBody[] CreateBodies(CancellationToken cancellationToken)
        {
            var dir = m_MathUtils.CreateVector(Direction.ToArray()) as MathVector;

            var bodies = new List<ISwTempBody>();

            foreach (var profile in Profiles) 
            {
                var body = m_Modeler.CreateExtrudedBody((Body2)profile.PlanarSheetBody.Body, dir, Depth) as IBody2;

                if (body == null)
                {
                    throw new Exception("Failed to create extrusion");
                }

                bodies.Add(m_App.CreateObjectFromDispatch<SwTempBody>(body, null));
            }

            return bodies.ToArray();
        }
    }
}

//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
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
        new ISwPlanarRegion[] Profiles { get; set; }
    }

    internal class SwTempExtrusion : SwTempPrimitive, ISwTempExtrusion
    {
        IXPlanarRegion[] IXExtrusion.Profiles
        {
            get => Profiles;
            set => Profiles = value?.Cast<ISwPlanarRegion>().ToArray();
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

        public ISwPlanarRegion[] Profiles
        {
            get => m_Creator.CachedProperties.Get<ISwPlanarRegion[]>();
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
                var length = Depth;

                if (length == 0) 
                {
                    throw new Exception("Cannot create extrusion of 0 length");
                }

                var body = (IBody2)m_Modeler.CreateExtrudedBody((Body2)profile.PlanarSheetBody.Body, dir, length);

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

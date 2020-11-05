using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Primitives
{
    public interface ISwTempExtrusion : IXExtrusion, ISwTempPrimitive
    {
        new ISwTempRegion[] Profiles { get; set; }
    }

    internal class SwTempExtrusion : SwTempPrimitive, ISwTempExtrusion
    {
        IXRegion[] IXExtrusion.Profiles
        {
            get => Profiles;
            set => Profiles = value?.Cast<ISwTempRegion>().ToArray();
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

        public ISwTempRegion[] Profiles
        {
            get => m_Creator.CachedProperties.Get<ISwTempRegion[]>();
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

        internal SwTempExtrusion(IMathUtility mathUtils, IModeler modeler, SwTempBody[] bodies, bool isCreated) 
            : base(mathUtils, modeler, bodies, isCreated)
        {
        }

        protected override ISwTempBody[] CreateBodies()
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

                bodies.Add(SwSelObject.FromDispatch<SwTempBody>(body));
            }

            return bodies.ToArray();
        }
    }
}

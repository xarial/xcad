using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Geometry.Primitives
{
    public interface ISwTempSweep : IXSweep, ISwTempPrimitive
    {

    }

    internal class SwTempSweep : SwTempPrimitive, ISwTempSweep
    {
        IXRegion IXSweep.Profile
        {
            get => Profile;
            set => Profile = (ISwTempRegion)value;
        }

        IXSegment IXSweep.Path
        {
            get => Path;
            set => Path = (SwCurve)value;
        }

        private readonly SwPart m_Part;

        internal SwTempSweep(SwPart part, IMathUtility mathUtils, IModeler modeler, SwTempBody body, bool isCreated)
            : base(mathUtils, modeler, body, isCreated)
        {
            m_Part = part;
        }

        public ISwTempRegion Profile
        {
            get => m_Creator.CachedProperties.Get<ISwTempRegion>();
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

        public SwCurve Path
        {
            get => m_Creator.CachedProperties.Get<SwCurve>();
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

        protected override SwTempBody CreateBody()
        {
            var selMgr = m_Part.Model.ISelectionManager;
            var selData = selMgr.CreateSelectData();

            IBody2 body = null;

            using (var selGrp = new SelectionGroup(selMgr))
            {
                var profileCurve = GetSingleCurve(Profile.Boundary.SelectMany(c => c.Curves).ToArray());
                var profileBody = profileCurve.CreateWireBody();
                selData.Mark = 1;
                selGrp.AddRange(profileBody.GetEdges() as object[], selData);

                var pathCurve = GetSingleCurve(Path.Curves);
                var pathBody = pathCurve.CreateWireBody();
                selData.Mark = 4;
                selGrp.AddRange(pathBody.GetEdges() as object[], selData);

                body = m_Modeler.CreateSweptBody((ModelDoc2)m_Part.Model, true, false,
                    (int)swTwistControlType_e.swTwistControlFollowPath,
                    true, false,
                    (int)swTangencyType_e.swTangencyNone,
                    (int)swTangencyType_e.swTangencyNone, false, 0, 0,
                    (int)swThinWallType_e.swThinWallMidPlane, 0, 0, false);
            }

            if (body == null) 
            {
                throw new Exception("Failed to create swept body");
            }

            return SwSelObject.FromDispatch<SwTempBody>(body);
        }

        private ICurve GetSingleCurve(ICurve[] curves)
        {
            ICurve curve;

            if (curves.Length == 1)
            {
                curve = curves.First();
            }
            else
            {
                curve = m_Modeler.MergeCurves(curves);

                if (curve == null)
                {
                    throw new Exception("Failed to merge curves");
                }
            }

            return curve;
        }
    }
}

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Features
{
    public interface ISwCoordinateSystem : IXCoordinateSystem, ISwFeature
    {
    }

    internal class SwCoordinateSystem : SwFeature, ISwCoordinateSystem
    {
        private readonly ICoordinateSystemFeatureData m_CoordSys;

        internal SwCoordinateSystem(ISwDocument doc, IFeature feat, bool created) : base(doc, feat, created)
        {
            m_CoordSys = feat.GetDefinition() as ICoordinateSystemFeatureData;
        }

        public TransformMatrix Transform
            => m_CoordSys.Transform.ToTransformMatrix();
    }
}

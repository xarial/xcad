//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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
        ICoordinateSystemFeatureData CoordSys { get; }
    }

    internal class SwCoordinateSystem : SwFeature, ISwCoordinateSystem
    {
        public ICoordinateSystemFeatureData CoordSys { get; }

        internal SwCoordinateSystem(IFeature feat, ISwDocument doc, ISwApplication app, bool created) : base(feat, doc, app, created)
        {
            CoordSys = feat.GetDefinition() as ICoordinateSystemFeatureData;
        }

        public override object Dispatch => CoordSys;

        public TransformMatrix Transform
            => CoordSys.Transform.ToTransformMatrix();
    }
}

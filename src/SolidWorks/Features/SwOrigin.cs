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
using System.Threading;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Features
{
    public interface ISwOrigin : IXOrigin, ISwFeature 
    {
    }

    internal class SwOrigin : SwFeature, ISwOrigin
    {
        internal const string TypeName = "OriginProfileFeature";

        internal SwOrigin(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(feat, doc, app, created)
        {
        }

        public override bool IsUserFeature => false;

        protected override IFeature InsertFeature(CancellationToken cancellationToken)
            => throw new NotSupportedException("Origin is a default feature and cannot be created");
    }
}

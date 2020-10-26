//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.Utils.Diagnostics;

namespace Xarial.XCad.SolidWorks.Documents
{
    public class SwPart : SwDocument3D, IXPart
    {
        public IPartDoc Part { get; }

        public IXBodyRepository Bodies { get; }

        internal SwPart(IPartDoc part, SwApplication app, IXLogger logger)
            : base((IModelDoc2)part, app, logger)
        {
            Part = part;
            Bodies = new SwPartBodyCollection(this);
        }

        public override Box3D CalculateBoundingBox()
        {
            var bodies = Part.GetBodies2((int)swBodyType_e.swAllBodies, true) as object[];

            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double minZ = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;
            double maxZ = double.MinValue;

            if (bodies?.Any() == true)
            {
                foreach (IBody2 swBody in bodies)
                {
                    double x;
                    double y;
                    double z;

                    swBody.GetExtremePoint(1, 0, 0, out x, out y, out z);

                    if (x > maxX)
                    {
                        maxX = x;
                    }

                    swBody.GetExtremePoint(-1, 0, 0, out x, out y, out z);

                    if (x < minX)
                    {
                        minX = x;
                    }

                    swBody.GetExtremePoint(0, 1, 0, out x, out y, out z);

                    if (y > maxY)
                    {
                        maxY = y;
                    }

                    swBody.GetExtremePoint(0, -1, 0, out x, out y, out z);

                    if (y < minY)
                    {
                        minY = y;
                    }

                    swBody.GetExtremePoint(0, 0, 1, out x, out y, out z);

                    if (z > maxZ)
                    {
                        maxZ = z;
                    }

                    swBody.GetExtremePoint(0, 0, -1, out x, out y, out z);

                    if (z < minZ)
                    {
                        minZ = z;
                    }
                }
            }

            return new Box3D(minX, minY, minZ, maxX, maxY, maxZ);
        }
    }

    internal class SwPartBodyCollection : SwBodyCollection
    {
        private IPartDoc m_Part;

        public SwPartBodyCollection(SwPart rootDoc) : base(rootDoc)
        {
            m_Part = rootDoc.Part;
        }

        protected override IEnumerable<IBody2> GetSwBodies()
            => (m_Part.GetBodies2((int)swBodyType_e.swAllBodies, false) as object[])?.Cast<IBody2>();
    }
}
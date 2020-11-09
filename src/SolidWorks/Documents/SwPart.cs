//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
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
    public interface ISwPart : ISwDocument3D, IXPart 
    {
        IPartDoc Part { get; }
    }

    internal class SwPart : SwDocument3D, ISwPart
    {
        public IPartDoc Part => Model as IPartDoc;

        public IXBodyRepository Bodies { get; }

        internal SwPart(IPartDoc part, ISwApplication app, IXLogger logger, bool isCreated)
            : base((IModelDoc2)part, app, logger, isCreated)
        {
            Bodies = new SwPartBodyCollection(this);
        }

        internal protected override swDocumentTypes_e? DocumentType => swDocumentTypes_e.swDocPART;

        protected override bool IsRapidMode => throw new NotSupportedException();

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
        private SwPart m_Part;

        public SwPartBodyCollection(SwPart rootDoc) : base(rootDoc)
        {
            m_Part = rootDoc;
        }

        protected override IEnumerable<IBody2> GetSwBodies()
            => (m_Part.Part.GetBodies2((int)swBodyType_e.swAllBodies, false) as object[])?.Cast<IBody2>();
    }
}
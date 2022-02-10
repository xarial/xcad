//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Primitives
{
    public interface ISwTempPlanarSheet : IXPlanarSheet, ISwTempPrimitive
    {
        new ISwTempPlanarSheetBody[] Bodies { get; }
        new ISwRegion Region { get; set; }
    }

    internal class SwTempPlanarSheet : SwTempPrimitive, ISwTempPlanarSheet
    {
        IXPlanarSheetBody[] IXPlanarSheet.Bodies => Bodies;

        IXRegion IXPlanarSheet.Region
        {
            get => Region;
            set => Region = (ISwRegion)value;
        }
        
        internal SwTempPlanarSheet(SwTempBody[] bodies, ISwApplication app, bool isCreated)
            : base(bodies, app, isCreated)
        {
        }

        public ISwRegion Region
        {
            get => m_Creator.CachedProperties.Get<ISwRegion>();
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

        new public ISwTempPlanarSheetBody[] Bodies => base.Bodies.Cast<ISwTempPlanarSheetBody>().ToArray();

        protected override ISwTempBody[] CreateBodies(CancellationToken cancellationToken)
        {
            Body2 sheetBody;

            if (Region is ISwFace)
            {
                sheetBody = ((ISwFace)Region).Face.ICreateSheetBody();
            }
            else 
            {
                var plane = Region.Plane;

                var planarSurf = m_Modeler.CreatePlanarSurface2(
                        plane.Point.ToArray(), plane.Normal.ToArray(), plane.Direction.ToArray()) as ISurface;

                if (planarSurf == null)
                {
                    throw new Exception("Failed to create plane");
                }

                var boundary = new List<ICurve>();

                for (int i = 0; i < Region.Boundary.Length; i++)
                {
                    boundary.AddRange(Region.Boundary[i].Curves);

                    if (i != Region.Boundary.Length - 1)
                    {
                        boundary.Add(null);
                    }
                }

                sheetBody = planarSurf.CreateTrimmedSheet4(boundary.ToArray(), true) as Body2;
            }

            if (sheetBody == null)
            {
                throw new Exception("Failed to create profile sheet body");
            }

            return new ISwTempBody[] { m_App.CreateObjectFromDispatch<SwTempBody>(sheetBody, null) };
        }
    }
}

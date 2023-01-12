//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
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
using Xarial.XCad.SolidWorks.Services;

namespace Xarial.XCad.SolidWorks.Geometry.Primitives
{
    public interface ISwTempPlanarSheet : IXPlanarSheet, ISwTempPrimitive
    {
        new ISwTempPlanarSheetBody[] Bodies { get; }
        new ISwPlanarRegion Region { get; set; }
    }

    internal class SwTempPlanarSheet : SwTempPrimitive, ISwTempPlanarSheet
    {
        IXPlanarSheetBody[] IXPlanarSheet.Bodies => Bodies;

        IXPlanarRegion IXPlanarSheet.Region
        {
            get => Region;
            set => Region = (ISwPlanarRegion)value;
        }

        private readonly IMemoryGeometryBuilderToleranceProvider m_TolProvider;

        internal SwTempPlanarSheet(SwTempBody[] bodies, SwApplication app, bool isCreated, IMemoryGeometryBuilderToleranceProvider tolProvider)
            : base(bodies, app, isCreated)
        {
            m_TolProvider = tolProvider;
        }

        public ISwPlanarRegion Region
        {
            get => m_Creator.CachedProperties.Get<ISwPlanarRegion>();
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
            => new ISwTempBody[] { Region.PlanarSheetBody };
    }
}

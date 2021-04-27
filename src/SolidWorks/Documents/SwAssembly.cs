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
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Utils.Diagnostics;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwAssembly : ISwDocument3D, IXAssembly
    {
        IAssemblyDoc Assembly { get; }
    }

    internal class SwAssembly : SwDocument3D, ISwAssembly
    {
        public IAssemblyDoc Assembly => Model as IAssemblyDoc;

        public IXComponentRepository Components { get; }
        
        internal SwAssembly(IAssemblyDoc assembly, SwApplication app, IXLogger logger, bool isCreated)
            : base((IModelDoc2)assembly, app, logger, isCreated)
        {
            Components = new SwAssemblyComponentCollection(this);
        }

        internal protected override swDocumentTypes_e? DocumentType => swDocumentTypes_e.swDocASSEMBLY;

        protected override bool IsRapidMode => Assembly.GetLightWeightComponentCount() > 0;

        public override Box3D CalculateBoundingBox()
        {
            const int NO_REF_GEOM = 0;

            var box = Assembly.GetBox(NO_REF_GEOM) as double[];

            return new Box3D(box[0], box[1], box[2], box[3], box[4], box[5]);
        }
    }

    internal class SwAssemblyComponentCollection : SwComponentCollection
    {
        private readonly ISwAssembly m_Assm;

        public SwAssemblyComponentCollection(ISwAssembly assm) : base(assm)
        {
            m_Assm = assm;
        }

        protected override int GetTotalChildrenCount()
            => m_Assm.Assembly.GetComponentCount(false);
        
        protected override IEnumerable<IComponent2> GetChildren()
            => (m_Assm.Assembly.GetComponents(true) as object[])?.Cast<IComponent2>();

        protected override int GetChildrenCount() 
            => m_Assm.Assembly.GetComponentCount(true);
    }
}
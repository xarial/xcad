//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Inventor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Inventor.Documents
{
    public interface IAiPart : IAiDocument, IXPart 
    {
        PartDocument Part { get; }
    }

    internal class AiPart : AiDocument3D, IAiPart
    {
        public PartDocument Part { get; }

        private readonly AiPartTable m_iPartTable;

        internal AiPart(PartDocument part, AiApplication ownerApp) : base((Document)part, ownerApp)
        {
            Part = part;
            m_iPartTable = new AiPartTable(this);
        }

        public IXBodyRepository Bodies => throw new NotImplementedException();

        IXModelView3DRepository IXDocument3D.ModelViews => throw new NotImplementedException();

        IXConfigurationRepository IXDocument3D.Configurations => m_iPartTable;

        IXPartConfigurationRepository IXPart.Configurations => m_iPartTable;

        public override IXConfigurationRepository Configurations => m_iPartTable;

        public IXMaterial Material { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}

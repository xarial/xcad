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

        internal AiPart(PartDocument part, AiApplication ownerApp) : base((Document)part, ownerApp)
        {
            Part = part;
        }

        public IXBodyRepository Bodies => throw new NotImplementedException();

        IXModelView3DRepository IXDocument3D.ModelViews => throw new NotImplementedException();

        IXConfigurationRepository IXDocument3D.Configurations => throw new NotImplementedException();

        IXPartConfigurationRepository IXPart.Configurations => throw new NotImplementedException();
    }
}

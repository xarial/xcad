using Inventor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Inventor.Documents
{
    public interface IAiDocument3D : IAiDocument, IXDocument3D
    {
    }

    internal abstract class AiDocument3D : AiDocument, IAiDocument3D
    {
        internal AiDocument3D(Document doc3D, AiApplication ownerApp) : base(doc3D, ownerApp)
        {
        }

        public IXDocumentEvaluation Evaluation => throw new NotImplementedException();

        public IXDocumentGraphics Graphics => throw new NotImplementedException();

        public IXConfigurationRepository Configurations => throw new NotImplementedException();

        IXModelView3DRepository IXDocument3D.ModelViews => throw new NotImplementedException();

        TSelObject IXObjectContainer.ConvertObject<TSelObject>(TSelObject obj)
        {
            throw new NotImplementedException();
        }
    }
}

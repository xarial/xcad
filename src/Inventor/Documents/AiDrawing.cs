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
    public interface IAiDrawing : IAiDocument, IXDrawing
    {
        DrawingDocument Drawing { get; }
    }

    internal class AiDrawing : AiDocument, IAiDrawing
    {
        public DrawingDocument Drawing { get; }

        internal AiDrawing(DrawingDocument drw, AiApplication ownerApp) : base((Document)drw, ownerApp)
        {
            Drawing = drw;
        }

        public IXSheetRepository Sheets => throw new NotImplementedException();

        IXDrawingOptions IXDrawing.Options => throw new NotImplementedException();

        IXDrawingSaveOperation IXDrawing.PreCreateSaveAsOperation(string filePath)
        {
            var translator = TryGetTranslator(filePath);

            if (translator != null)
            {
                return new AiDrawingTranslatorSaveOperation(this, translator, filePath);
            }
            else
            {
                return new AiDrawingSaveOperation(this, filePath);
            }
        }

        public override IXSaveOperation PreCreateSaveAsOperation(string filePath) => ((IXDrawing)this).PreCreateSaveAsOperation(filePath);
    }
}

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
    }
}

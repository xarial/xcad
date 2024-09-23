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
using Xarial.XCad.Documents.Structures;
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

        public IXLayerRepository Layers => throw new NotImplementedException();

        IXDrawingSaveOperation IXDrawing.PreCreateSaveAsOperation(string filePath)
        {
            var translator = TryGetTranslator(filePath);

            if (translator != null)
            {
                switch (translator.ClientId)
                {
                    case "{C24E3AC2-122E-11D5-8E91-0010B541CD80}":
                    case "{C24E3AC4-122E-11D5-8E91-0010B541CD80}":
                        return new AiDxfDwgSaveOperation(this, translator, filePath);

                    default:
                        return new AiDrawingTranslatorSaveOperation(this, translator, filePath);
                }
            }
            else
            {
                return new AiDrawingSaveOperation(this, filePath);
            }
        }

        public override IXSaveOperation PreCreateSaveAsOperation(string filePath) => ((IXDrawing)this).PreCreateSaveAsOperation(filePath);
    }
}

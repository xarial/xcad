using Inventor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Annotations;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Features;
using Xarial.XCad.Toolkit;
using Xarial.XCad.UI;

namespace Xarial.XCad.Inventor.Documents
{
    /// <summary>
    /// Autodesk Inventor specific sheet
    /// </summary>
    public interface IAiSheet : IXSheet, IAiObject
    {
        /// <summary>
        /// Pointer to sheet
        /// </summary>
        Sheet Sheet { get; }
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class AiSheet : AiObject, IAiSheet
    {
        internal static AiSheet New(Sheet sheet, AiDrawing drw, AiApplication app)
            => new AiSheet(sheet, drw, app);

        protected AiSheet(Sheet sheet, AiDrawing drw, AiApplication app) : base(sheet, drw, app)
        {
            Sheet = sheet;
        }

        public Sheet Sheet { get; }

        public IXIdentifier Id => new XIdentifier(Sheet.InternalName);

        public string Name 
        {
            get => Sheet.Name; 
            set => Sheet.Name = value; 
        }

        public IXDrawingViewRepository DrawingViews => throw new NotImplementedException();

        public IXAnnotationRepository Annotations => throw new NotImplementedException();

        public IXSketch2D Sketch => throw new NotImplementedException();

        public IXSheetFormat Format => throw new NotImplementedException();

        public IXImage Preview => throw new NotImplementedException();

        public Scale Scale { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public PaperSize PaperSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ViewsProjectionType_e ViewsProjectionType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsSelected => throw new NotImplementedException();

        public IXSheet Clone(IXDrawing targetDrawing)
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void Select(bool append)
        {
            throw new NotImplementedException();
        }
    }
}

using Inventor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XCad.Inventor.Documents;

namespace Xarial.XCad.Inventor.Geometry
{
    /// <summary>
    /// Autodesk Inventor specific body
    /// </summary>
    public interface IAiBody : IXBody 
    {
        /// <summary>
        /// Pointer to body
        /// </summary>
        SurfaceBody Body { get; }
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal abstract class AiBody : AiSelObject, IAiBody
    {
        protected AiBody(SurfaceBody body, AiPart part, AiApplication app) : base(body, part, app)
        {
            Body = body;
        }

        public SurfaceBody Body { get; }

        public string Name 
        {
            get => Body.Name; 
            set => Body.Name = value; 
        }

        public bool Visible
        {
            get => Body.Visible; 
            set => Body.Visible = value; 
        }

        public IXComponent Component => throw new NotImplementedException();

        public IEnumerable<IXFace> Faces => throw new NotImplementedException();

        public IEnumerable<IXEdge> Edges => throw new NotImplementedException();

        public IXMaterial Material { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public System.Drawing.Color? Color { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IXMemoryBody Copy()
        {
            throw new NotImplementedException();
        }
    }

    internal class AiSolidBody : AiBody, IXSolidBody
    {
        internal static AiSolidBody New(SurfaceBody body, AiPart part, AiApplication app) 
            => new AiSolidBody(body, part, app);

        public double Volume => Body.Volume[0.01];

        protected AiSolidBody(SurfaceBody body, AiPart part, AiApplication app) : base(body, part, app)
        {
            if (!body.IsSolid) 
            {
                throw new Exception("Body is not solid body");
            }
        }
    }

    internal class AiSheetBody : AiBody, IXSheetBody
    {
        internal static AiSheetBody New(SurfaceBody body, AiPart part, AiApplication app)
            => new AiSheetBody(body, part, app);

        private AiSheetBody(SurfaceBody body, AiPart part, AiApplication app) : base(body, part, app)
        {
            if (body.IsSolid)
            {
                throw new Exception("Body is not surface body");
            }
        }
    }

    internal class AiSheetMetalBody : AiSolidBody, IXSheetMetalBody
    {
        internal static new AiSheetMetalBody New(SurfaceBody body, AiPart part, AiApplication app)
            => new AiSheetMetalBody(body, part, app);

        private AiSheetMetalBody(SurfaceBody body, AiPart part, AiApplication app) : base(body, part, app)
        {
            if (!(body.ComponentDefinition is SheetMetalComponentDefinition))
            {
                throw new Exception("Body does not belong to sheet metal part");
            }
        }
    }
}

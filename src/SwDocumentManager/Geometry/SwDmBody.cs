//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SwDocumentManager.Documents;

namespace Xarial.XCad.SwDocumentManager.Geometry
{
    public interface ISwDmBody : IXBody, ISwDmObject
    {
    }

    internal abstract class SwDmBody : SwDmSelObject, ISwDmBody
    {
        #region Not Supported
        public string Name { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public bool Visible { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public Color? Color { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public IEnumerable<IXFace> Faces => throw new NotSupportedException();
        public IEnumerable<IXEdge> Edges => throw new NotSupportedException();
        public IXComponent Component => throw new NotSupportedException();
        public IXMemoryBody Copy() => throw new NotSupportedException();
        public void Transform(TransformMatrix transform) => throw new NotSupportedException();
        public IXMaterial Material { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        #endregion

        public SwDmBody(SwDmPart part) : base(null, part.OwnerApplication, part)
        {
        }
    }
}

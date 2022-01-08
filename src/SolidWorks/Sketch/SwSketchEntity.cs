//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Drawing;
using Xarial.XCad.Features;
using Xarial.XCad.Services;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public interface ISwSketchEntity : IXSketchEntity 
    {
    }

    internal abstract class SwSketchEntity : SwSelObject, ISwSketchEntity
    {
        public new abstract bool IsCommitted { get; }
        public abstract Color? Color { get; set; }

        public abstract IXSketchBase OwnerSketch { get; }

        internal SwSketchEntity(object ent, ISwDocument doc, ISwApplication app) : base(ent, doc, app)
        {
        }
    }
}
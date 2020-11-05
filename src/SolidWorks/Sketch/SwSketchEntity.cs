//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Drawing;
using Xarial.XCad.Services;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public abstract class SwSketchEntity : SwSelObject, IXSketchEntity
    {
        public abstract bool IsCommitted { get; }
        public abstract Color? Color { get; set; }

        protected readonly ISwDocument m_Doc;
        
        internal SwSketchEntity(ISwDocument doc, object ent) : base(doc.Model, ent)
        {
            m_Doc = doc;
        }
    }
}
//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Sketch
{
    /// <summary>
    /// SOLIDWORKS specific sketch block definition
    /// </summary>
    public interface ISwSketchBlockDefinition : IXSketchBlockDefinition, ISwFeature
    {
        /// <summary>
        /// Pointer to sketch block definition
        /// </summary>
        ISketchBlockDefinition SketchBlockDefinition { get; }
    }

    internal class SwSketchBlockDefinition : SwFeature, ISwSketchBlockDefinition
    {
        public ISketchBlockDefinition SketchBlockDefinition { get; }

        public IXSketchEntityRepository Entities { get; }

        public override bool IsAlive => this.CheckIsAlive(() => { var test = SketchBlockDefinition.LinkToFile; });

        public Point InsertionPoint => new Point((double[])SketchBlockDefinition.InsertionPoint.ArrayData);

        public IXSketchBlockInstanceRepository Instances { get; }

        internal SwSketchBlockDefinition(IFeature feat, SwDocument doc, SwApplication app, bool created) 
            : base(feat, doc, app, created) 
        {
            SketchBlockDefinition = (ISketchBlockDefinition)feat.GetSpecificFeature2();

            Instances = new SwSketchBlockInstanceCollection(this);

            Entities = new SwSketchEntityCollection(doc.CreateObjectFromDispatch<SwSketchBase>(SketchBlockDefinition.GetSketch()), doc, app);
        }
    }
}

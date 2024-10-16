//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Annotations;
using Xarial.XCad.Documents;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Annotations
{
    /// <summary>
    /// SOLIDWORKS-specific break line
    /// </summary>
    public interface ISwBreakLine : IXBreakLine, ISwAnnotation
    {
        /// <summary>
        /// Pointer to break line
        /// </summary>
        IBreakLine BreakLine { get; }
    }

    internal class SwBreakLine : SwAnnotation, ISwBreakLine
    {
        private readonly IBreakLine m_BreakLine;

        internal SwBreakLine(IBreakLine breakLine, SwDocument doc, SwApplication app) 
            : base(breakLine != null ? new NotSupportedAnnotation(breakLine) : default(IAnnotation), doc, app)
        {
            m_BreakLine = breakLine;
        }

        public override bool IsCommitted => m_BreakLine != null;

        public override object Dispatch => BreakLine;

        public override IXLayer Layer
        {
            get => SwLayerHelper.GetLayer(this, x => x.m_BreakLine.Layer);
            set => SwLayerHelper.SetLayer(this, value, (x, y) => x.m_BreakLine.Layer = y);
        }

        public IBreakLine BreakLine => IsCommitted ? m_BreakLine : throw new NonCommittedElementAccessException();

        internal override void Select(bool append, ISelectData selData) => BreakLine.Select(append, (SelectData)selData);
    }
}

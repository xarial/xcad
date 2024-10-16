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
    /// SOLIDWORKS-specific section line
    /// </summary>
    public interface ISwSectionLine : IXSectionLine, ISwAnnotation
    {
        /// <summary>
        /// Pointer to section
        /// </summary>
        IDrSection Section { get; }
    }

    internal class SwSectionLine : SwAnnotation, ISwSectionLine
    {
        private readonly IDrSection m_Section;

        internal SwSectionLine(IDrSection section, SwDocument doc, SwApplication app) 
            : base(section != null ? new NotSupportedAnnotation(section) : default(IAnnotation), doc, app)
        {
            m_Section = section;
        }

        public override bool IsCommitted => m_Section != null;

        public override object Dispatch => Section;

        public override IXLayer Layer
        {
            get => SwLayerHelper.GetLayer(this, x => x.Section.Layer);
            set => SwLayerHelper.SetLayer(this, value, (x, y) => x.Section.Layer = y);
        }

        public IDrSection Section => IsCommitted ? m_Section : throw new NonCommittedElementAccessException();

        public Line Definition
        {
            get
            {
                if (!IsCommitted)
                {
                    return m_Creator.CachedProperties.Get<Line>();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        internal override void Select(bool append, ISelectData selData)
        {
            if (!((ISwDocument)OwnerDocument).Model.Extension.SelectByID2(
                Section.GetName(), "SECTIONLINE", 0, 0, 0,
                append, selData?.Mark ?? -1, selData?.Callout, (int)swSelectOption_e.swSelectOptionDefault))
            {
                throw new Exception($"Failed to select section line: '{Section.GetName()}'");
            }
        }
    }
}

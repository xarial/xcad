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
    /// SOLIDWORKS-specific detail circle
    /// </summary>
    public interface ISwDetailCircle : IXDetailCircle, ISwAnnotation
    {
        /// <summary>
        /// Pointer to detail circle
        /// </summary>
        IDetailCircle Circle { get; }
    }

    internal class SwDetailCircle : SwAnnotation, ISwDetailCircle
    {
        private readonly IDetailCircle m_Circle;

        internal SwDetailCircle(IDetailCircle circle, SwDocument doc, SwApplication app) 
            : base(circle != null ? new NotSupportedAnnotation(circle) : default(IAnnotation), doc, app)
        {
            m_Circle = circle;
        }

        public override bool IsCommitted => m_Circle != null;

        public override object Dispatch => Circle;

        public override IXLayer Layer
        {
            get => SwLayerHelper.GetLayer(this, x => x.Circle.Layer);
            set => SwLayerHelper.SetLayer(this, value, (x, y) => x.Circle.Layer = y);
        }

        internal override void Select(bool append, ISelectData selData)
        {
            if (!Circle.Select(append, (SelectData)selData)) //NOTE: IDetailCircle::Select does not work for the view copied to another drawing file
            {
                if (!((ISwDocument)OwnerDocument).Model.Extension.SelectByID2(
                    Circle.GetName(), "DETAILCIRCLE", 0, 0, 0, 
                    append, selData?.Mark ?? -1, selData?.Callout, (int)swSelectOption_e.swSelectOptionDefault))
                {
                    throw new Exception($"Failed to select detail view: '{Circle.GetName()}'");
                }
            }
        }

        public IDetailCircle Circle => IsCommitted ? m_Circle : throw new NonCommittedElementAccessException();

        public Circle Definition
        {
            get
            {
                if (!IsCommitted)
                {
                    return m_Creator.CachedProperties.Get<Circle>();
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
    }
}

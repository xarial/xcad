//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Annotations
{
    public interface ISwNote : IXNote, ISwAnnotation
    {
        INote Note { get; }
    }

    [DebuggerDisplay("{" + nameof(Text) + "}")]
    internal class SwNote : SwAnnotation, ISwNote
    {
        public INote Note => m_Note;

        public override object Dispatch => Note;

        private INote m_Note;

        internal SwNote(INote note, SwDocument doc, SwApplication app) : base(note?.IGetAnnotation(), doc, app)
        {
            m_Note = note;
        }

        public string Text 
        {
            get
            {
                if (IsCommitted)
                {
                    return Note.GetText();
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<string>();
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    if (!Note.SetText(value))
                    {
                        throw new Exception("Failed to set the note text value");
                    }
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public Box3D Box 
        {
            get 
            {
                var extent = (double[])Note.GetExtent();
                return new Box3D(extent[0], extent[1], extent[2], extent[3], extent[4], extent[5]);
            }
        }

        public double Angle 
        {
            get 
            {
                if (IsCommitted)
                {
                    return Note.Angle;
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<double>();
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    Note.Angle = value;
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        protected override IAnnotation CreateAnnotation(CancellationToken arg)
        {
            m_Note = (INote)OwnerDocument.Model.InsertNote(Text);

            var ann = m_Note.IGetAnnotation();

            if (Position != null) 
            {
                SetPosition(ann, Position);
            }

            if (Color != null) 
            {
                SetColor(ann, Color);
            }

            if (m_Creator.CachedProperties.Has<double>(nameof(Angle))) 
            {
                m_Note.Angle = Angle;
            }

            if (Font != null) 
            {
                var textFormat = (ITextFormat)ann.GetTextFormat(0);

                SwFontHelper.FillTextFormat(Font, textFormat);

                ann.SetTextFormat(0, false, textFormat);
            }

            return ann;
        }
    }
}

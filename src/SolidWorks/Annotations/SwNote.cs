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
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Enums;
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

        public TextJustification_e TextJustification
        {
            get
            {
                if (IsCommitted)
                {
                    switch ((swTextJustification_e)Note.GetTextJustification()) 
                    {
                        case swTextJustification_e.swTextJustificationNone:
                            return TextJustification_e.None;

                        case swTextJustification_e.swTextJustificationLeft:
                            return TextJustification_e.Left;

                        case swTextJustification_e.swTextJustificationRight:
                            return TextJustification_e.Right;

                        case swTextJustification_e.swTextJustificationCenter:
                            return TextJustification_e.Center;

                        default:
                            throw new NotSupportedException();
                    }
                }
                else
                {
                    return m_Creator.CachedProperties.Get<TextJustification_e>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    SetTextJustification(Note, Annotation, value);
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

            if (m_Creator.CachedProperties.Has<TextJustification_e>(nameof(TextJustification)))
            {
                SetTextJustification(m_Note, ann, TextJustification);
            }

            if (Font != null) 
            {
                var textFormat = (ITextFormat)ann.GetTextFormat(0);

                SwFontHelper.FillTextFormat(Font, textFormat);

                ann.SetTextFormat(0, false, textFormat);
            }

            return ann;
        }

        private void SetTextJustification(INote note, IAnnotation ann, TextJustification_e textJust)
        {
            swTextJustification_e textJustSw;

            switch (textJust)
            {
                case TextJustification_e.None:
                    textJustSw = swTextJustification_e.swTextJustificationNone;
                    break;

                case TextJustification_e.Left:
                    textJustSw = swTextJustification_e.swTextJustificationLeft;
                    break;

                case TextJustification_e.Right:
                    textJustSw = swTextJustification_e.swTextJustificationRight;
                    break;

                case TextJustification_e.Center:
                    textJustSw = swTextJustification_e.swTextJustificationCenter;
                    break;

                default:
                    throw new NotSupportedException();
            }

            note.SetTextJustification((int)textJustSw);

            //NOTE: boundary of the note does not update until note is refreshed (e.g. hidden/shown, selected)
            base.Refresh(ann);
        }
    }
}

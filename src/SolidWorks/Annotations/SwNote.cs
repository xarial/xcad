//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Annotations;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Annotations
{
    public interface ISwNote : IXNote 
    {
        INote Note { get; }
    }

    internal class SwNote : SwSelObject, ISwNote
    {
        public INote Note { get; }

        internal SwNote(INote note, ISwDocument doc, ISwApplication app) : base(note, doc, app)
        {
            Note = note;
        }

        public string Text 
        {
            get => Note.GetText();
            set 
            {
                if (!Note.SetText(value)) 
                {
                    throw new Exception("Failed to set the note text value");
                }
            }
        }
    }
}

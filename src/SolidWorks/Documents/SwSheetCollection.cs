//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.SolidWorks.Documents.EventHandlers;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwSheetCollection : IXSheetRepository, IDisposable 
    {
    }

    internal class SwSheetCollection : ISwSheetCollection
    {
        public event SheetActivatedDelegate SheetActivated 
        {
            add 
            {
                m_SheetActivatedEventsHandler.Attach(value);
            }
            remove 
            {
                m_SheetActivatedEventsHandler.Detach(value);
            }
        }

        private readonly ISwApplication m_App;
        private readonly SwDrawing m_Drawing;

        private readonly SheetActivatedEventsHandler m_SheetActivatedEventsHandler;

        internal SwSheetCollection(SwDrawing doc, ISwApplication app)
        {
            m_App = app;
            m_Drawing = doc;
            m_SheetActivatedEventsHandler = new SheetActivatedEventsHandler(doc, app);
        }

        public IXSheet this[string name] => this.Get(name);

        public bool TryGet(string name, out IXSheet ent)
        {
            var sheet = m_Drawing.Drawing.Sheet[name];

            if (sheet != null)
            {
                ent = m_Drawing.CreateObjectFromDispatch<SwSheet>(sheet);
                return true;
            }
            else 
            {
                ent = null;
                return false;
            }
        }

        public int Count => (m_Drawing.Drawing.GetSheetNames() as string[]).Length;

        public IXSheet Active
        {
            get 
            {
                if (m_Drawing.IsCommitted)
                {
                    return m_Drawing.CreateObjectFromDispatch<SwSheet>((ISheet)m_Drawing.Drawing.GetCurrentSheet());
                }
                else 
                {
                    return new UncommittedPreviewOnlySheet(m_Drawing, m_App);
                }
            }
        }
        
        public void AddRange(IEnumerable<IXConfiguration> ents)
        {
            throw new NotImplementedException();
        }

        public void AddRange(IEnumerable<IXSheet> ents)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public void RemoveRange(IEnumerable<IXConfiguration> ents)
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<IXSheet> ents)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<IXSheet> GetEnumerator() => new SwSheetEnumerator(m_Drawing);
    }

    internal class SwSheetEnumerator : IEnumerator<IXSheet>
    {
        public IXSheet Current
            => m_Doc.CreateObjectFromDispatch<SwSheet>(m_Doc.Drawing.Sheet[m_SheetNames[m_CurSheetIndex]]);

        object IEnumerator.Current => Current;

        private int m_CurSheetIndex;

        private readonly SwDrawing m_Doc;

        private string[] m_SheetNames;

        internal SwSheetEnumerator(SwDrawing doc)
        {
            m_Doc = doc;

            m_CurSheetIndex = -1;
            m_SheetNames = (string[])m_Doc.Drawing.GetSheetNames();
        }

        public bool MoveNext()
        {
            m_CurSheetIndex++;
            return m_CurSheetIndex < m_SheetNames.Length;
        }

        public void Reset()
        {
            m_CurSheetIndex = -1;
        }

        public void Dispose()
        {
        }
    }
}

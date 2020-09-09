//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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
    public class SwSheetCollection : IXSheetRepository, IDisposable
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

        private readonly SwDrawing m_Doc;

        private readonly SheetActivatedEventsHandler m_SheetActivatedEventsHandler;

        internal SwSheetCollection(SwDrawing doc)
        {
            m_Doc = doc;
            m_SheetActivatedEventsHandler = new SheetActivatedEventsHandler(doc);
        }

        public IXSheet this[string name]
        {
            get 
            {
                if (TryGet(name, out IXSheet sheet))
                {
                    return sheet;
                }
                else 
                {
                    throw new KeyNotFoundException($"Failed to find '{name}'");
                }
            }
        }

        public bool TryGet(string name, out IXSheet ent)
        {
            var sheet = m_Doc.Drawing.Sheet[name];

            if (sheet != null)
            {
                ent = SwObject.FromDispatch<SwSheet>(sheet, m_Doc);
                return true;
            }
            else 
            {
                ent = null;
                return false;
            }
        }

        public int Count => (m_Doc.Drawing.GetSheetNames() as string[]).Length;

        public IXSheet Active
            => SwObject.FromDispatch<SwSheet>((ISheet)m_Doc.Drawing.GetCurrentSheet(), m_Doc);
        
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

        public IEnumerator<IXSheet> GetEnumerator() => new SwSheetEnumerator(m_Doc);
    }

    internal class SwSheetEnumerator : IEnumerator<IXSheet>
    {
        public IXSheet Current
            => SwObject.FromDispatch<SwSheet>(m_Doc.Drawing.Sheet[m_SheetNames[m_CurSheetIndex]], m_Doc);

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

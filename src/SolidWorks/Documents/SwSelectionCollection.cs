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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.SolidWorks.Documents.EventHandlers;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwSelectionCollection : IXSelectionRepository, IDisposable 
    {
    }

    internal class SwSelectionCollection : ISwSelectionCollection
    {
        private readonly SwDocument m_Doc;
        private IModelDoc2 Model => m_Doc.Model;
        private ISelectionMgr SelMgr => Model.ISelectionManager;

        private readonly NewSelectionEventHandler m_NewSelectionEventHandler;
        private readonly ClearSelectionEventHandler m_ClearSelectionEventHandler;

        public event NewSelectionDelegate NewSelection 
        {
            add 
            {
                m_NewSelectionEventHandler.Attach(value);
            }
            remove 
            {
                m_NewSelectionEventHandler.Detach(value);
            }
        }

        public event ClearSelectionDelegate ClearSelection
        {
            add
            {
                m_ClearSelectionEventHandler.Attach(value);
            }
            remove
            {
                m_ClearSelectionEventHandler.Detach(value);
            }
        }

        public int Count => SelMgr.GetSelectedObjectCount2(-1);

        public IXSelObject this[string name] => throw new NotSupportedException();

        internal SwSelectionCollection(SwDocument doc, ISwApplication app) 
        {
            m_Doc = doc;

            m_NewSelectionEventHandler = new NewSelectionEventHandler(doc, app);
            m_ClearSelectionEventHandler = new ClearSelectionEventHandler(doc, app);
        }

        public void AddRange(IEnumerable<IXSelObject> ents)
        {
            if (ents == null) 
            {
                throw new ArgumentNullException(nameof(ents));
            }

            var disps = ents.Cast<SwSelObject>().Select(e => new DispatchWrapper(e.Dispatch)).ToArray();

            var curSelCount = SelMgr.GetSelectedObjectCount2(-1);

            var selCount = Model.Extension.MultiSelect2(disps, true, null) - curSelCount;

            if (selCount != disps.Length) 
            {
                throw new Exception("Selection failed");
            }
        }

        public void Clear()
        {
            Model.ClearSelection2(true);
        }

        public IEnumerator<IXSelObject> GetEnumerator()
        {
            return new SwSelObjectEnumerator(m_Doc, SelMgr);
        }

        public void RemoveRange(IEnumerable<IXSelObject> ents)
        {
            //TODO: implement
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose()
        {
            m_NewSelectionEventHandler.Dispose();
            m_ClearSelectionEventHandler.Dispose();
        }

        public bool TryGet(string name, out IXSelObject ent)
        {
            throw new NotSupportedException();
        }
    }

    internal class SwSelObjectEnumerator : IEnumerator<IXSelObject>
    {
        public IXSelObject Current => m_Doc.CreateObjectFromDispatch<ISwSelObject>(m_SelMgr.GetSelectedObject6(m_CurSelIndex, -1));

        object IEnumerator.Current => Current;

        private int m_CurSelIndex;

        private readonly SwDocument m_Doc;
        private readonly ISelectionMgr m_SelMgr;

        internal SwSelObjectEnumerator(SwDocument doc, ISelectionMgr selMgr) 
        {
            m_CurSelIndex = 0;
            m_Doc = doc;
            m_SelMgr = selMgr;
        }

        public bool MoveNext()
        {
            m_CurSelIndex++;
            return m_SelMgr.GetSelectedObjectCount2(-1) >= m_CurSelIndex;
        }

        public void Reset()
        {
            m_CurSelIndex = 1;
        }

        public void Dispose()
        {
        }
    }
}

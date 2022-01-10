//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.SolidWorks.Documents.EventHandlers;
using Xarial.XCad.SolidWorks.Utils;

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

        private readonly ISwApplication m_App;

        internal SwSelectionCollection(SwDocument doc, ISwApplication app) 
        {
            m_Doc = doc;
            m_App = app;

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
            => Model.ClearSelection2(true);

        public IEnumerator<IXSelObject> GetEnumerator()
            => new SwSelObjectEnumerator(m_Doc, SelMgr);

        public void RemoveRange(IEnumerable<IXSelObject> ents)
        {
            const int RESULT_TRUE = 1;

            var selMgr = Model.ISelectionManager;

            var entsToDeSelect = ents.Cast<ISwSelObject>().ToList();

            for (int i = selMgr.GetSelectedObjectCount2(-1); i >= 1; i--)
            {
                var entToDeSelect = entsToDeSelect.FirstOrDefault(
                    e => m_App.Sw.IsSame(selMgr.GetSelectedObject6(i, -1), e.Dispatch) == (int)swObjectEquality.swObjectSame);
                
                if (entToDeSelect != null)
                {
                    entsToDeSelect.Remove(entToDeSelect);

                    if (selMgr.DeSelect2(i, -1) != RESULT_TRUE) 
                    {
                        throw new Exception($"Failed to deselect entity at index {i}");
                    }
                }
            }

            if (entsToDeSelect.Any()) 
            {
                throw new Exception($"Failed to deselect {entsToDeSelect.Count} entities as hose were not selected");
            }
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

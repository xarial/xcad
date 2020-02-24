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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Xarial.XCad.Documents;

namespace Xarial.XCad.SolidWorks.Documents
{
    public class SwSelectionCollection : IXSelectionRepository
    {
        private readonly IModelDoc2 m_Model;
        private readonly ISelectionMgr m_SelMgr;

        public int Count => m_SelMgr.GetSelectedObjectCount2(-1);

        public IXSelObject this[string name] => throw new NotSupportedException();

        internal SwSelectionCollection(IModelDoc2 model) 
        {
            m_Model = model;
            m_SelMgr = m_Model.ISelectionManager;
        }

        public void AddRange(IEnumerable<IXSelObject> ents)
        {
            if (ents == null) 
            {
                throw new ArgumentNullException(nameof(ents));
            }

            var disps = ents.Cast<SwSelObject>().Select(e => new DispatchWrapper(e.Dispatch)).ToArray();

            var selCount = m_Model.Extension.MultiSelect2(disps, true, null);

            if (selCount != disps.Length) 
            {
                throw new Exception("Selection failed");
            }
        }

        public void Clear()
        {
            m_Model.ClearSelection2(true);
        }

        public IEnumerator<IXSelObject> GetEnumerator()
        {
            return new SwSelObjectEnumerator(m_SelMgr);
        }

        public void RemoveRange(IEnumerable<IXSelObject> ents)
        {
            //TODO: implement
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class SwSelObjectEnumerator : IEnumerator<IXSelObject>
    {
        public IXSelObject Current => SwObject.FromDispatch<SwSelObject>(m_SelMgr.GetSelectedObject6(m_CurSelIndex, -1));

        object IEnumerator.Current => Current;

        private int m_CurSelIndex;

        private readonly ISelectionMgr m_SelMgr;

        internal SwSelObjectEnumerator(ISelectionMgr selMgr) 
        {
            m_CurSelIndex = 0;
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

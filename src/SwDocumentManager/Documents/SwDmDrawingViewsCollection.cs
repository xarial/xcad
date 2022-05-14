//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Base;
using SolidWorks.Interop.swdocumentmgr;
using System.Linq;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmDrawingViewsCollection : IXDrawingViewRepository 
    {
    }

    internal class SwDmDrawingViewsCollection : ISwDmDrawingViewsCollection
    {
        #region Not Supported
        public void AddRange(IEnumerable<IXDrawingView> ents) => throw new NotSupportedException();
        public void RemoveRange(IEnumerable<IXDrawingView> ents) => throw new NotSupportedException();
        public T PreCreate<T>() where T : IXDrawingView => throw new NotSupportedException();
        #endregion

        public IXDrawingView this[string name] => this.Get(name);

        public int Count => (((ISwDMSheet4)m_Sheet.Sheet).GetViews() as object[] ?? new object[0]).Length;

        private readonly SwDmSheet m_Sheet;

        private readonly SwDmDrawing m_Drw;

        internal SwDmDrawingViewsCollection(SwDmSheet sheet, SwDmDrawing drw) 
        {
            m_Sheet = sheet;
            m_Drw = drw;
        }

        public IEnumerator<IXDrawingView> GetEnumerator()
        {
            var views = ((ISwDMSheet4)m_Sheet.Sheet).GetViews() as object[] ?? new object[0];

            return views.Cast<ISwDMView>().Select(v => SwDmObjectFactory.FromDispatch<ISwDmDrawingView>(v, m_Drw)).GetEnumerator();
        }

        public bool TryGet(string name, out IXDrawingView ent)
        {
            ent = this.FirstOrDefault(v => string.Equals(v.Name, name, StringComparison.CurrentCultureIgnoreCase));
            return ent != null;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}

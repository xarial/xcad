﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Base;
using System.Threading;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmSheetCollection : IXSheetRepository
    {
    }

    internal class SwDmSheetCollection : ISwDmSheetCollection
    {
        #region Not Supported
        public event SheetActivatedDelegate SheetActivated { add => throw new NotSupportedException(); remove => throw new NotSupportedException(); }
        public event SheetCreatedDelegate SheetCreated { add => throw new NotSupportedException(); remove => throw new NotSupportedException(); }
        public void AddRange(IEnumerable<IXSheet> ents, CancellationToken cancellationToken) => throw new NotSupportedException();
        public void RemoveRange(IEnumerable<IXSheet> ents, CancellationToken cancellationToken) => throw new NotSupportedException();
        public T PreCreate<T>() where T : IXSheet => throw new NotSupportedException();
        #endregion Not Supported

        private readonly SwDmDrawing m_Drw;

        internal SwDmSheetCollection(SwDmDrawing drw)
        {
            m_Drw = drw;
        }

        public IXSheet this[string name] => RepositoryHelper.Get(this, name);

        public IXSheet Active
        {
            get
            {
                var activeSheetName = (m_Drw.Document as ISwDMDocument10).GetActiveSheetName();
                return this[activeSheetName];
            }
            set => throw new NotSupportedException();
        }

        public int Count => (m_Drw.Document as ISwDMDocument10).GetSheetCount();

        public IEnumerator<IXSheet> GetEnumerator() => new SwDmSheetEnumerator(m_Drw);

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) => RepositoryHelper.FilterDefault(this, filters, reverseOrder);

        public bool TryGet(string name, out IXSheet ent)
        {
            ent = this.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            return ent != null;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    internal class SwDmSheetEnumerator : IEnumerator<IXSheet>
    {
        public IXSheet Current
            => SwDmObjectFactory.FromDispatch<ISwDmSheet>(m_Sheets[m_CurSheetIndex], m_Drw);

        object IEnumerator.Current => Current;

        private int m_CurSheetIndex;

        private readonly SwDmDrawing m_Drw;

        private ISwDMSheet[] m_Sheets;

        internal SwDmSheetEnumerator(SwDmDrawing drw)
        {
            m_Drw = drw;

            m_CurSheetIndex = -1;
            m_Sheets = (((ISwDMDocument10)m_Drw.Document).GetSheets() as object[])?.Cast<ISwDMSheet>().ToArray();
        }

        public bool MoveNext()
        {
            m_CurSheetIndex++;
            return m_CurSheetIndex < m_Sheets.Length;
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

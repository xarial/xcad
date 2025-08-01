//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Inventor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.Inventor.Documents
{
    internal class AiSheetsCollection : IXSheetRepository
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly AiDrawing m_Drw;

        private readonly RepositoryHelper<IXSheet> m_RepoHelper;

        internal AiSheetsCollection(AiDrawing drw) 
        {
            m_Drw = drw;
            m_RepoHelper = new RepositoryHelper<IXSheet>(this);
        }

        public IXSheet this[string name] => m_RepoHelper.Get(name);

        public IXSheet Active 
        {
            get => m_Drw.CreateObjectFromDispatch<IAiSheet>(m_Drw.Drawing.ActiveSheet); 
            set => ((IAiSheet)value).Sheet.Activate(); 
        }

        public int Count => m_Drw.Drawing.Sheets.Count;

        public event SheetActivatedDelegate SheetActivated;
        public event SheetCreatedDelegate SheetCreated;

        public void AddRange(IEnumerable<IXSheet> ents, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) 
            => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public IEnumerator<IXSheet> GetEnumerator()
        {
            foreach (Sheet sheet in m_Drw.Drawing.Sheets)
            {
                yield return m_Drw.CreateObjectFromDispatch<IAiSheet>(sheet);
            }
        }

        public T PreCreate<T>() where T : IXSheet
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<IXSheet> ents, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public bool TryGet(string name, out IXSheet ent)
        {
            foreach (Sheet sheet in m_Drw.Drawing.Sheets) 
            {
                if (string.Equals(sheet.Name, name, StringComparison.CurrentCultureIgnoreCase)) 
                {
                    ent = m_Drw.CreateObjectFromDispatch<IAiSheet>(sheet);
                    return true;
                }
            }

            ent = null;
            return false;
        }
    }
}

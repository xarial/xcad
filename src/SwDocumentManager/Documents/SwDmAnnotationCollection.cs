using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.SwDocumentManager.Annotations;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    internal class SwDmAnnotationCollection : IXAnnotationRepository
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region Not Supported
        public void AddRange(IEnumerable<IXAnnotation> ents, CancellationToken cancellationToken) => throw new NotSupportedException();
        public T PreCreate<T>() where T : IXAnnotation => throw new NotSupportedException();
        public void RemoveRange(IEnumerable<IXAnnotation> ents, CancellationToken cancellationToken) => throw new NotSupportedException();
        #endregion

        private readonly SwDmDocument m_Doc;

        private readonly RepositoryHelper<IXAnnotation> m_RepoHelper;

        internal SwDmAnnotationCollection(SwDmDocument doc) 
        {
            m_Doc = doc;

            m_RepoHelper = new RepositoryHelper<IXAnnotation>(this);
        }

        public IXAnnotation this[string name] => m_RepoHelper.Get(name);

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters)
        {
            bool bomTable;
            bool revTable;

            if (filters?.Any() == true)
            {
                bomTable = false;
                revTable = false;

                foreach (var filter in filters)
                {
                    if (typeof(IXBomTable).IsAssignableFrom(filter.Type))
                    {
                        bomTable = true;
                    }
                    else if (filter.Type == null || typeof(IXTable).IsAssignableFrom(filter.Type) || typeof(IXAnnotation).IsAssignableFrom(filter.Type))
                    {
                        bomTable = true;
                        revTable = true;
                        break;
                    }
                }
            }
            else
            {
                bomTable = true;
                revTable = true;
            }

            foreach (var ent in m_RepoHelper.FilterDefault(IterateAnnotations(bomTable, revTable), filters, reverseOrder))
            {
                yield return ent;
            }
        }

        public int Count => IterateAnnotations(true, true).Count();

        public IEnumerator<IXAnnotation> GetEnumerator() => IterateAnnotations(true, true).GetEnumerator();

        public bool TryGet(string name, out IXAnnotation ent)
        {
            var table = ((ISwDMDocument10)m_Doc.Document).GetTable(name);

            if (table != null)
            {
                if (IsInScope(table))
                {
                    ent = m_Doc.CreateObjectFromDispatch<ISwDmTable>(table);
                    return true;
                }
            }

            ent = null;
            return false;
        }

        private IEnumerable<ISwDmAnnotation> IterateAnnotations(bool bomTable, bool revTable)
        {
            var filters = new List<SwDmTableType>();

            if (bomTable)
            {
                filters.Add(SwDmTableType.swDmTableTypeBOM);
                filters.Add(SwDmTableType.swDmTableTypeBOMHidden);
            }

            if (revTable) 
            {
                filters.Add(SwDmTableType.swDmTableTypeRevision);
            }

            foreach (var filter in filters)
            {
                var tableNames = (string[])((ISwDMDocument10)m_Doc.Document).GetTableNames(filter);

                if (tableNames != null) 
                {
                    foreach (var tableName in tableNames) 
                    {
                        var table = ((ISwDMDocument10)m_Doc.Document).GetTable(tableName);

                        if (table != null)
                        {
                            if (IsInScope(table))
                            {
                                yield return m_Doc.CreateObjectFromDispatch<ISwDmTable>(table);
                            }
                        }
                        else 
                        {
                            throw new NullReferenceException($"Failed to get table by name '{tableName}'");
                        }
                    }
                }
            }
        }

        protected virtual bool IsInScope(object annDisp) 
        {
            if (annDisp is ISwDMTable) 
            {
                return true;
            }

            return false;
        }
    }

    internal class SwDmSheetAnnotationCollection : SwDmAnnotationCollection
    {
        private readonly SwDmSheet m_Sheet;

        internal SwDmSheetAnnotationCollection(SwDmSheet sheet, SwDmDocument doc) : base(doc)
        {
            m_Sheet = sheet;
        }

        protected override bool IsInScope(object annDisp)
        {
            if (base.IsInScope(annDisp)) 
            {
                if (annDisp is ISwDMTable5)
                {
                    var ownerSheet = ((ISwDMTable5)annDisp).Sheet;

                    return m_Sheet.Sheet == ownerSheet;
                }
            }

            return false;
        }
    }
}

//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.SolidWorks.Documents.EventHandlers;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwSheetCollection : IXSheetRepository, IDisposable 
    {
        new ISwSheet Active { get; set; }
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

        IXSheet IXSheetRepository.Active { get => Active; set => Active = (ISwSheet)value; }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly SwApplication m_App;
        private readonly SwDrawing m_Drawing;

        private readonly SheetActivatedEventsHandler m_SheetActivatedEventsHandler;

        private readonly List<IXSheet> m_Cache;

        private readonly SwTemplatePlaceholderSheet m_TemplatePlaceholderSheet;

        internal SwSheetCollection(SwDrawing doc, SwApplication app)
        {
            m_App = app;
            m_Drawing = doc;
            m_SheetActivatedEventsHandler = new SheetActivatedEventsHandler(doc, app);
            m_Cache = new List<IXSheet>();

            m_TemplatePlaceholderSheet = new SwTemplatePlaceholderSheet(doc, app);
        }

        public IXSheet this[string name] => RepositoryHelper.Get(this, name);

        public bool TryGet(string name, out IXSheet ent)
        {
            if (m_Drawing.IsCommitted)
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
            else 
            {
                ent = IterateCachedSheets().FirstOrDefault(s => string.Equals(s.Name, name));
                return ent != null;
            }
        }

        public int Count 
        {
            get 
            {
                if (m_Drawing.IsCommitted)
                {
                    return (m_Drawing.Drawing.GetSheetNames() as string[]).Length;
                }
                else 
                {
                    return m_Cache.Count;
                }
            }
        }

        public ISwSheet Active
        {
            get 
            {
                if (m_Drawing.IsCommitted)
                {
                    return m_Drawing.CreateObjectFromDispatch<SwSheet>(m_Drawing.Drawing.IGetCurrentSheet());
                }
                else 
                {
                    return new UncommittedPreviewOnlySheet(m_Drawing, m_App);
                }
            }
            set 
            {
                var currentSheet = m_Drawing.Drawing.IGetCurrentSheet();
                
                if (m_App.Sw.IsSame(currentSheet, value.Sheet) != (int)swObjectEquality.swObjectSame)
                {
                    if (!m_Drawing.Drawing.ActivateSheet(value.Name))
                    {
                        throw new Exception($"Failed to activate '{value.Name}'");
                    }
                }
            }
        }

        public void AddRange(IEnumerable<IXSheet> sheets, CancellationToken cancellationToken)
        {
            if (m_Drawing.IsCommitted)
            {
                RepositoryHelper.AddRange(this, sheets, cancellationToken);
            }
            else
            {
                m_Cache.AddRange(sheets);
            }
        }

        internal void CommitCache(CancellationToken cancellationToken)
        {
            try
            {
                if (m_Cache.Any())
                {
                    var curSheets = IterateSheets().ToArray();

                    if (curSheets.Length > m_Cache.Count)
                    {
                        var excessSheets = curSheets.Skip(m_Cache.Count).ToArray();
                        curSheets = curSheets.Except(excessSheets).ToArray();
                        RemoveRange(excessSheets, cancellationToken);
                    }
                    
                    SetupCurrentSheets(curSheets, m_Cache.Take(curSheets.Length).ToArray());

                    if (curSheets.Length < m_Cache.Count)
                    {
                        AddRange(m_Cache.Skip(curSheets.Length), cancellationToken);
                    }
                }
                else 
                {
                    foreach (SwSheet sheet in IterateSheets())
                    {
                        sheet.SetupSheet(m_TemplatePlaceholderSheet);
                    }
                }
            }
            finally
            {
                m_Cache.Clear();
            }
        }

        private void SetupCurrentSheets(IXSheet[] curSheets, IXSheet[] targetSheets)
        {
            //resolving potential name conflicts
            for (int i = 0; i < curSheets.Length; i++)
            {
                if (targetSheets.FirstOrDefault(x => string.Equals(x.Name, curSheets[i].Name, StringComparison.CurrentCultureIgnoreCase)) != null)
                {
                    var tempSheetName = Guid.NewGuid().ToString();
                    curSheets[i].Name = tempSheetName;
                }
            }

            for (int i = 0; i < curSheets.Length; i++) 
            {
                ((SwSheet)curSheets[i]).SetupSheet(targetSheets[i]);
            }
        }

        public void RemoveRange(IEnumerable<IXSheet> sheets, CancellationToken cancellationToken) 
        {
            if (m_Drawing.IsCommitted)
            {
                m_Drawing.Selections.Clear();

                foreach (ISwSheet sheet in sheets)
                {
                    sheet.Select(true);
                }

                if (!m_Drawing.Model.Extension.DeleteSelection2((int)swDeleteSelectionOptions_e.swDelete_Absorbed))
                {
                    throw new Exception($"Failed to delete sheets");
                }
            }
            else 
            {
                foreach (var sheet in sheets.ToArray()) 
                {
                    m_Cache.Remove(sheet);
                }
            }
        }

        public IEnumerator<IXSheet> GetEnumerator()
        {
            if (m_Drawing.IsCommitted)
            {
                return IterateSheets().GetEnumerator();
            }
            else
            {
                return IterateCachedSheets().GetEnumerator();
            }
        }

        public T PreCreate<T>() where T : IXSheet
            => RepositoryHelper.PreCreate<IXSheet, T>(this,
                () => new SwSheet(null, m_Drawing, m_Drawing.OwnerApplication));

        private IEnumerable<IXSheet> IterateSheets() 
        {
            var sheetNames = (string[])m_Drawing.Drawing.GetSheetNames();

            foreach (var sheetName in sheetNames) 
            {
                yield return m_Drawing.CreateObjectFromDispatch<SwSheet>(m_Drawing.Drawing.Sheet[sheetName]);
            }
        }

        private IEnumerable<IXSheet> IterateCachedSheets() 
        {
            if (m_Cache.Any())
            {
                foreach (var cachedSheet in m_Cache)
                {
                    yield return cachedSheet;
                }
            }
            else 
            {
                yield return m_TemplatePlaceholderSheet;
            }
        }

        public void Dispose()
        {
        }
    }
}

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
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwSheetCollection : IXSheetRepository, IDisposable 
    {
        new ISwSheet Active { get; set; }
    }

    internal class SwSheetsCache : EntityCache<IXSheet>
    {
        private readonly SwDrawing m_Drw;

        /// <summary>
        /// This is a placeholder for a sheet(s) which already present in the template
        /// </summary>
        /// <remarks>Drawing will always contain at least one sheet so this is returned if no user sheets are found</remarks>
        private readonly SwSheet m_TemplatePlaceholderSheet;

        public SwSheetsCache(SwDrawing drw, SwApplication app, IXRepository<IXSheet> repo, Func<IXSheet, string> nameProvider) : base(drw, repo, nameProvider)
        {
            m_Drw = drw;

            m_TemplatePlaceholderSheet = new SwSheet(null, drw, app);
        }

        protected override void CommitEntitiesFromCache(IReadOnlyList<IXSheet> ents, CancellationToken cancellationToken)
        {
            if (m_Drw.IsCommitted)
            {
                if (ents.Any())
                {
                    var curSheets = m_Repo.ToArray();

                    if (curSheets.Length > ents.Count)
                    {
                        var excessSheets = curSheets.Skip(ents.Count).ToArray();
                        curSheets = curSheets.Except(excessSheets).ToArray();
                        m_Repo.RemoveRange(excessSheets, cancellationToken);
                    }

                    SetupCurrentSheets(curSheets, ents.Take(curSheets.Length).ToArray(), cancellationToken);

                    if (curSheets.Length < ents.Count)
                    {
                        m_Repo.AddRange(ents.Skip(curSheets.Length), cancellationToken);
                    }
                }
                else
                {
                    var isFirst = true;

                    SwSheet placeholderSheetReplacement = null;

                    foreach (SwSheet sheet in m_Repo)
                    {
                        sheet.SetupSheet(m_TemplatePlaceholderSheet);

                        //only commiting drawing views to the first sheet from the placeholder in case template has more than one
                        if (isFirst)
                        {
                            placeholderSheetReplacement = sheet;

                            var placeholderSheetDrwViews = m_TemplatePlaceholderSheet.DrawingViews.ToArray();
                            
                            if(placeholderSheetDrwViews.Any())
                            {
                                //NOTE: need this call to propagate the sheet pointer to the drawing views otherwise they won't be committed
                                foreach (SwDrawingView drwView in placeholderSheetDrwViews) 
                                {
                                    drwView.SetParentSheet(sheet);
                                }
                                
                                sheet.DrawingViews.AddRange(placeholderSheetDrwViews);
                            }
                        }

                        isFirst = false;
                    }

                    m_TemplatePlaceholderSheet.SetFromExisting(placeholderSheetReplacement);
                }
            }
            else 
            {
                throw new Exception("Cached entities can only be committed to the loaded drawing");
            }
        }

        protected override IEnumerable<IXSheet> IterateEntities(IReadOnlyList<IXSheet> ents)
        {
            if (ents.Any())
            {
                foreach (var cachedSheet in ents)
                {
                    yield return cachedSheet;
                }
            }
            else
            {
                yield return m_TemplatePlaceholderSheet;
            }
        }

        private void SetupCurrentSheets(IXSheet[] curSheets, IXSheet[] targetSheets, CancellationToken cancellationToken)
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
                var swSheet = (SwSheet)curSheets[i];

                swSheet.SetupSheet(targetSheets[i]);

                ((SwSheet)targetSheets[i]).InitFromExisting(swSheet, cancellationToken);
            }
        }
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

        private readonly SwSheetsCache m_Cache;

        internal SwSheetCollection(SwDrawing doc, SwApplication app)
        {
            m_App = app;
            m_Drawing = doc;
            m_SheetActivatedEventsHandler = new SheetActivatedEventsHandler(doc, app);
            m_Cache = new SwSheetsCache(doc, app, this, s => s.Name);
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
                return m_Cache.TryGet(name, out ent);
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
                RepositoryHelper.AddRange(sheets, cancellationToken);
            }
            else
            {
                m_Cache.AddRange(sheets, cancellationToken);
            }
        }

        internal void CommitCache(CancellationToken cancellationToken) => m_Cache.Commit(cancellationToken);

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
                m_Cache.RemoveRange(sheets, cancellationToken);
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
                return m_Cache.GetEnumerator();
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

        public void Dispose()
        {
        }
    }
}

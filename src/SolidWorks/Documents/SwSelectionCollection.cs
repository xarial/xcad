//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
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
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI;
using Xarial.XCad.Toolkit;
using Xarial.XCad.SolidWorks.Graphics;
using Xarial.XCad.Graphics;
using System.Threading;
using Xarial.XCad.Toolkit.Utils;
using Xarial.XCad.Base;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwSelectionCollection : IXSelectionRepository, IDisposable 
    {
        ISwSelCallout PreCreateCallout<T>()
            where T : SwCalloutBaseHandler, new();
    }

    internal class SwSelectionCollection : ISwSelectionCollection
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly SwDocument m_Doc;
        private IModelDoc2 Model => m_Doc.Model;
        internal ISelectionMgr SelMgr => Model.ISelectionManager;

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

        public IXSelObject this[string name] => RepositoryHelper.Get(this, name);

        private readonly ISwApplication m_App;

        internal SwSelectionCollection(SwDocument doc, ISwApplication app) 
        {
            m_Doc = doc;
            m_App = app;
            
            m_NewSelectionEventHandler = new NewSelectionEventHandler(doc, app);
            m_ClearSelectionEventHandler = new ClearSelectionEventHandler(doc, app);
        }

        public void ReplaceRange(IEnumerable<IXSelObject> ents, CancellationToken cancellationToken)
            => MultiSelect(ents, cancellationToken, false);

        public void AddRange(IEnumerable<IXSelObject> ents, CancellationToken cancellationToken)
            => MultiSelect(ents, cancellationToken, true);

        private void MultiSelect(IEnumerable<IXSelObject> ents, CancellationToken cancellationToken, bool append) 
        {
            if (ents == null)
            {
                throw new ArgumentNullException(nameof(ents));
            }

            var disps = ents.Cast<SwSelObject>().Select(e => new DispatchWrapper(e.Dispatch)).ToArray();

            int curSelCount;

            if (append)
            {
                curSelCount = SelMgr.GetSelectedObjectCount2(-1);
            }
            else 
            {
                curSelCount = 0;
            }

            var selCount = Model.Extension.MultiSelect2(disps, append, null) - curSelCount;

            if (selCount != disps.Length)
            {
                throw new Exception("Selection failed");
            }
        }

        public void Clear() => Model.ClearSelection2(true);

        public IEnumerator<IXSelObject> GetEnumerator() => IterateSelection().GetEnumerator();

        public void RemoveRange(IEnumerable<IXSelObject> ents, CancellationToken cancellationToken)
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

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) => RepositoryHelper.FilterDefault(this, filters, reverseOrder);

        public bool TryGet(string name, out IXSelObject ent)
        {
            ent = IterateSelection().FirstOrDefault(
                s => s is IHasName && string.Equals(name, ((IHasName)s).Name, StringComparison.CurrentCultureIgnoreCase));

            return ent != null;
        }

        public IXSelCallout PreCreateCallout() 
            => new SwSelCallout(m_Doc, this, ((SwApplication)m_App).Services.GetService<ICalloutHandlerProvider>().CreateHandler(m_App));

        public ISwSelCallout PreCreateCallout<T>() where T : SwCalloutBaseHandler, new()
            => new SwSelCallout(m_Doc, this, new T());

        public T PreCreate<T>() where T : IXSelObject => throw new NotImplementedException();

        private IEnumerable<SwSelObject> IterateSelection() 
        {
            for (int i = 1; i < SelMgr.GetSelectedObjectCount2(-1) + 1; i++)
            {
                var selType = SelMgr.GetSelectedObjectType3(i, -1);

                if (selType != (int)swSelectType_e.swSelSELECTIONSETNODE && selType != (int)swSelectType_e.swSelBROWSERITEM) //selection node returns null as the object
                {
                    var disp = SelMgr.GetSelectedObject6(i, -1);

                    if (disp != null)
                    {
                        yield return m_Doc.CreateObjectFromDispatch<SwSelObject>(disp);
                    }
                    else 
                    {
                        System.Diagnostics.Debug.Assert(false, "Object does not have dispatch. Add to the types exceptions above");
                    }
                }
            }
        }

        public void Dispose()
        {
            m_NewSelectionEventHandler.Dispose();
            m_ClearSelectionEventHandler.Dispose();
        }
    }
}

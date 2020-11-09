//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI;
using SolidWorks.Interop.sldworks;
using Xarial.XCad.Documents;

namespace Xarial.XCad.SolidWorks.UI
{
    public interface ISwModelViewTab<TControl> : IXCustomPanel<TControl>, IDisposable 
    {
    }

    internal class SwModelViewTab<TControl> : ISwModelViewTab<TControl>
    {
        public bool IsActive
        {
            get => m_MdlViewMgr.IsControlTabActive(m_Title);
            set
            {
                if (!m_MdlViewMgr.ActivateControlTab(m_Title)) 
                {
                    throw new Exception("Failed to activate the model view tab");
                }
            }
        }

        public TControl Control { get; }

        private readonly string m_Title;
        private readonly ModelViewManager m_MdlViewMgr;

        private readonly Documents.ISwDocument m_Doc;

        private bool m_IsDisposed;

        internal SwModelViewTab(TControl ctrl, string title, ModelViewManager mdlViewMgr, Documents.ISwDocument doc) 
        {
            Control = ctrl;
            m_Title = title;
            m_MdlViewMgr = mdlViewMgr;
            m_Doc = doc;
            m_Doc.Closing += OnDestroyed;

            m_IsDisposed = false;
        }

        private void OnDestroyed(IXDocument doc)
        {
            Dispose();
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            if (!m_IsDisposed)
            {
                m_Doc.Closing -= OnDestroyed;

                m_IsDisposed = true;

                if (Control is IDisposable)
                {
                    (Control as IDisposable).Dispose();
                }

                var res = m_MdlViewMgr.DeleteControlTab(m_Title);

                //TODO: log result
            }
        }
    }
}

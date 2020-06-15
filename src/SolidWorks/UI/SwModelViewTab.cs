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

namespace Xarial.XCad.SolidWorks.UI
{
    public class SwModelViewTab<TControl> : IXCustomPanel<TControl>, IDisposable
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

        private readonly Documents.SwDocument m_Doc;

        private bool m_IsDisposed;

        internal SwModelViewTab(TControl ctrl, string title, ModelViewManager mdlViewMgr, Documents.SwDocument doc) 
        {
            Control = ctrl;
            m_Title = title;
            m_MdlViewMgr = mdlViewMgr;
            m_Doc = doc;
            m_Doc.Destroyed += OnDestroyed;

            m_IsDisposed = false;
        }

        private void OnDestroyed(IModelDoc2 obj)
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

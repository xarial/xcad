//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.UI
{
    public class SwFeatureMgrTab<TControl> : IXCustomPanel<TControl>, IDisposable
    {
        public bool IsActive
        {
            get => throw new NotSupportedException();
            set => m_FeatViewMgr.ActivateView();
        }

        public TControl Control { get; }

        private readonly IFeatMgrView m_FeatViewMgr;

        private readonly Documents.SwDocument m_Doc;

        private bool m_IsDisposed;

        internal SwFeatureMgrTab(TControl ctrl, IFeatMgrView featMgrView, Documents.SwDocument doc)
        {
            Control = ctrl;
            m_FeatViewMgr = featMgrView;
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

                if (!m_FeatViewMgr.DeleteView()) 
                {
                    //TODO: log result
                }
            }
        }
    }
}

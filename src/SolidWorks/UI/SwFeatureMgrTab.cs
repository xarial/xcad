//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.UI
{
    public interface ISwFeatureMgrTab<TControl> : IXCustomPanel<TControl>, IDisposable 
    {
    }

    internal class SwFeatureMgrTab<TControl> : ISwFeatureMgrTab<TControl>
    {
        public bool IsActive
        {
            get => throw new NotSupportedException();
            set => m_FeatViewMgr.ActivateView();
        }

        public TControl Control { get; }

        private readonly IFeatMgrView m_FeatViewMgr;
        private readonly SwDocument m_Doc;
        private readonly IXLogger m_Logger;

        private bool m_IsDisposed;

        internal SwFeatureMgrTab(TControl ctrl, IFeatMgrView featMgrView, SwDocument doc, IXLogger logger)
        {
            Control = ctrl;
            m_FeatViewMgr = featMgrView;
            m_Doc = doc;
            m_Doc.Destroyed += OnDestroyed;

            m_Logger = logger;

            m_IsDisposed = false;
        }

        private void OnDestroyed(IXDocument doc)
        {
            try
            {
                Dispose();
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
            }
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            if (!m_IsDisposed)
            {
                m_Doc.Destroyed -= OnDestroyed;

                m_IsDisposed = true;

                if (Control is IDisposable)
                {
                    try
                    {
                        (Control as IDisposable).Dispose();
                    }
                    finally 
                    {
                        if (!m_FeatViewMgr.DeleteView())
                        {
                            m_Logger.Log("Failed to delete feature manager view");
                        }
                    }
                }
            }
        }
    }
}

//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.UI;
using Xarial.XCad.UI.Exceptions;

namespace Xarial.XCad.SolidWorks.UI.Toolkit
{
    internal abstract class DocumentAttachedCustomPanel<TControl> : IXCustomPanel<TControl>
    {
        public event ControlCreatedDelegate<TControl> ControlCreated;
        public event PanelActivatedDelegate<TControl> Activated;

        public bool IsActive
        {
            get
            {
                if (IsControlCreated)
                {
                    return GetIsActive();
                }
                else
                {
                    throw new CustomPanelControlNotCreatedException();
                }
            }
            set
            {
                if (IsControlCreated)
                {
                    SetIsActive(value);
                }
                else
                {
                    throw new CustomPanelControlNotCreatedException();
                }
            }
        }

        public TControl Control
        {
            get
            {
                if (IsControlCreated)
                {
                    return m_CurControl;
                }
                else
                {
                    throw new CustomPanelControlNotCreatedException();
                }
            }
            set
            {
                m_CurControl = value;
            }
        }

        protected abstract bool GetIsActive();
        protected abstract void SetIsActive(bool active);

        private TControl m_CurControl;

        private bool m_IsControlCreated;

        public bool IsControlCreated
        {
            get => m_IsControlCreated;
            private set
            {
                m_IsControlCreated = value;
                if (value)
                {
                    this.ControlCreated?.Invoke(Control);
                }
            }
        }

        protected readonly SwDocument m_Doc;
        protected readonly IXLogger m_Logger;

        private bool m_IsDisposed;

        internal DocumentAttachedCustomPanel(SwDocument doc, IXLogger logger)
        {
            m_Doc = doc;
            m_Doc.Hidden += OnHidden;
            m_Doc.Destroyed += OnDestroyed;
            m_Doc.App.Documents.DocumentActivated += OnDocumentActivated;

            m_Logger = logger;

            m_IsDisposed = false;
        }

        internal void InitControl()
        {
            if (m_Doc.Model.Visible)
            {
                m_CurControl = CreateControl();
                IsControlCreated = true;
            }
        }

        protected abstract TControl CreateControl();
        protected abstract void DeleteControl();

        protected void RaiseActivated() 
        {
            Activated?.Invoke(this);
        }

        private void OnDocumentActivated(IXDocument doc)
        {
            if (!m_IsDisposed)
            {
                if (m_Doc == doc)
                {
                    if (!IsControlCreated)
                    {
                        m_CurControl = CreateControl();
                        IsControlCreated = true;
                    }
                }
            }
            else 
            {
                Debug.Assert(false, "This event should be unsubscribed in dispose");
            }
        }

        private void OnHidden(SwDocument doc)
        {
            try
            {
                DisposeControl();
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
            }
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
                m_Doc.App.Documents.DocumentActivated -= OnDocumentActivated;
                m_Doc.Hidden -= OnHidden;
                m_Doc.Destroyed -= OnDestroyed;

                m_IsDisposed = true;
                DisposeControl();
            }
        }

        private void DisposeControl()
        {
            if (IsControlCreated)
            {
                try
                {
                    if (Control is IDisposable)
                    {
                        (Control as IDisposable).Dispose();
                    }
                }
                finally
                {
                    DeleteControl();
                }

                IsControlCreated = false;
            }
        }
    }
}

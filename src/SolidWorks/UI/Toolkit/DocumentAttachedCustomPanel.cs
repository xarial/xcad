//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.UI;
using Xarial.XCad.UI.Exceptions;

namespace Xarial.XCad.SolidWorks.UI.Toolkit
{
    internal abstract class DocumentAttachedCustomPanel<TControl> : IXCustomPanel<TControl>, IAutoDisposable
    {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event Action<IAutoDisposable> Disposed;

        public event ControlCreatedDelegate<TControl> ControlCreated;
        public virtual event PanelActivatedDelegate<TControl> Activated;

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

        private readonly ISwApplication m_App;
        protected readonly SwDocument m_Doc;
        protected readonly IXLogger m_Logger;

        private bool m_IsDisposed;

        internal DocumentAttachedCustomPanel(SwDocument doc, ISwApplication app, IXLogger logger)
        {
            m_App = app;
            m_Doc = doc;
            
            m_Doc.Hidden += OnHidden;
            m_Doc.Destroyed += OnDestroyed;

            app.Documents.DocumentActivated += OnDocumentActivated;

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

        private void OnDocumentActivated(IXDocument doc)
        {
            if (!m_IsDisposed)
            {
                if (m_Doc.Equals(doc))
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

        private void OnHidden(IXDocument doc)
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

        public void Dispose() => Close();

        public virtual void Close()
        {
            if (!m_IsDisposed)
            {
                m_App.Documents.DocumentActivated -= OnDocumentActivated;
                m_Doc.Hidden -= OnHidden;
                m_Doc.Destroyed -= OnDestroyed;

                m_IsDisposed = true;
                DisposeControl();

                Disposed?.Invoke(this);
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

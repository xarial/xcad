//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swpublished;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Xarial.XCad.Extensions;
using Xarial.XCad.Extensions.Attributes;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Features.CustomFeature.Delegates;
using Xarial.XCad.SolidWorks.Features.CustomFeature;
using Xarial.XCad.SolidWorks.Features.CustomFeature.Toolkit;
using Xarial.XCad.SolidWorks.UI;
using Xarial.XCad.SolidWorks.UI.Commands;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.Utils.Diagnostics;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks
{
    /// <inheritdoc/>
    [ComVisible(true)]
    public abstract class SwAddInEx : IXExtension, ISwAddin, IDisposable
    {
        #region Registration

        private static RegistrationHelper m_RegHelper;

        /// <summary>
        /// COM Registration entry function
        /// </summary>
        /// <param name="t">Type</param>
        [ComRegisterFunction]
        public static void RegisterFunction(Type t)
        {
            if (t.TryGetAttribute<SkipRegistrationAttribute>()?.Skip != true)
            {
                GetRegistrationHelper(t).Register(t);
            }
        }

        /// <summary>
        /// COM Unregistration entry function
        /// </summary>
        /// <param name="t">Type</param>
        [ComUnregisterFunction]
        public static void UnregisterFunction(Type t)
        {
            if (t.TryGetAttribute<SkipRegistrationAttribute>()?.Skip != true)
            {
                GetRegistrationHelper(t).Unregister(t);
            }
        }

        private static RegistrationHelper GetRegistrationHelper(Type moduleType)
        {
            return m_RegHelper ?? (m_RegHelper = new RegistrationHelper(new TraceLogger(moduleType.FullName)));
        }

        #endregion Registration

        IXApplication IXExtension.Application => Application;
        IXCommandManager IXExtension.CommandManager => CommandManager;
        IXCustomPanel<TControl> IXExtension.CreateDocumentTab<TControl>(XCad.Documents.IXDocument doc) => CreateDocumentTab<TControl>((Documents.SwDocument)doc);

        private readonly ILogger m_Logger;

        public SwApplication Application { get; private set; }

        public SwCommandManager CommandManager { get; private set; }

        /// <summary>
        /// Add-ins cookie (id)
        /// </summary>
        protected int AddInId { get; private set; }

        public SwAddInEx()
        {
            m_Logger = new TraceLogger("XCad.AddIn");
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ConnectToSW(object ThisSW, int cookie)
        {
            m_Logger.Log("Loading add-in");

            try
            {
                var app = ThisSW as ISldWorks;
                AddInId = cookie;

                app.SetAddinCallbackInfo(0, this, AddInId);

                Application = new SwApplication(app, m_Logger);

                SwMacroFeatureDefinition.Application = Application;

                CommandManager = new SwCommandManager(Application, AddInId, m_Logger);

                OnConnect();

                return true;
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                return false;
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool DisconnectFromSW()
        {
            m_Logger.Log("Unloading add-in");

            try
            {
                OnDisconnect();
                Dispose();
                return true;
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                return false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Command click callback
        /// </summary>
        /// <param name="cmdId">Command tag</param>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnCommandClick(string cmdId)
        {
            CommandManager.HandleCommandClick(cmdId);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int OnCommandEnable(string cmdId)
        {
            return CommandManager.HandleCommandEnable(cmdId);
        }

        public virtual void OnConnect()
        {
        }

        public virtual void OnDisconnect()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CommandManager.Dispose();
                Application.Documents.Dispose();
                Application.Dispose();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        IXPropertyPage<TData> IXExtension.CreatePage<TData>()
        {
            return CreatePropertyManagerPage<TData>(typeof(TData));
        }

        public SwPropertyManagerPage<TData> CreatePage<TData>()
        {
            return CreatePropertyManagerPage<TData>(typeof(TData));
        }

        public SwPropertyManagerPage<TData> CreatePage<TData, THandler>()
            where THandler : SwPropertyManagerPageHandler, new()
        {
            return CreatePropertyManagerPage<TData>(typeof(THandler));
        }

        private SwPropertyManagerPage<TData> CreatePropertyManagerPage<TData>(Type handlerType)
        {
            return new SwPropertyManagerPage<TData>(Application, m_Logger, handlerType);
        }

        public SwModelViewTab<TControl> CreateDocumentTab<TControl>(Documents.SwDocument doc)
        {
#if NET461
            if (typeof(System.Windows.Forms.Control).IsAssignableFrom(typeof(TControl)))
            {
                if (typeof(System.Windows.Forms.UserControl).IsAssignableFrom(typeof(TControl)) && typeof(TControl).IsComVisible())
                {
                    //TODO: create COM control
                    throw new NotImplementedException();
                }
                else
                {
                    var winCtrl = (System.Windows.Forms.Control)Activator.CreateInstance(typeof(TControl));

                    return CreateTabFromControl<TControl>(winCtrl, doc, (TControl)(object)winCtrl);
                }
            }
            else if (typeof(System.Windows.UIElement).IsAssignableFrom(typeof(TControl)))
            {
                var wpfCtrl = (System.Windows.Controls.Control)Activator.CreateInstance(typeof(TControl));
                var host = new System.Windows.Forms.Integration.ElementHost();
                host.Child = wpfCtrl;

                return CreateTabFromControl<TControl>(host, doc, (TControl)(object)wpfCtrl);
            }
            else 
            {
                throw new NotSupportedException($"Only System.Windows.Forms.Control or System.Windows.UIElement are supported");
            }
#else
            throw new NotSupportedException();
#endif
        }

#if NET461
        private SwModelViewTab<TControl> CreateTabFromControl<TControl>(System.Windows.Forms.Control host, Documents.SwDocument doc, TControl ctrl) 
        {
            var title = "";

            if (typeof(TControl).TryGetAttribute(out DisplayNameAttribute att)) 
            {
                title = att.DisplayName;
            }

            if (string.IsNullOrEmpty(title)) 
            {
                title = typeof(TControl).Name;
            }

            var mdlViewMgr = doc.Model.ModelViewManager;
            
            if (mdlViewMgr.DisplayWindowFromHandlex64(title, host.Handle.ToInt64(), true))
            {
                return new SwModelViewTab<TControl>(ctrl, title, mdlViewMgr, doc);
            }
            else 
            {
                throw new Exception("Failed to create control");
            }
        }
#endif
    }
}
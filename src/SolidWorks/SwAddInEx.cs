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
using Xarial.XCad.SolidWorks.UI.Commands;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.Utils.Diagnostics;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks
{
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

        private readonly ILogger m_Logger;
        private SwApplication m_Application;
        private SwCommandManager m_CommandManager;

        public IXApplication Application => m_Application;
        public IXCommandManager CommandManager => m_CommandManager;

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

                m_Application = new SwApplication(app, m_Logger);

                SwMacroFeatureDefinition.Application = m_Application;

                m_CommandManager = new SwCommandManager(m_Application, AddInId, m_Logger);

                return OnConnect();
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                throw;
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool DisconnectFromSW()
        {
            m_Logger.Log("Unloading add-in");

            try
            {
                var res = OnDisconnect();
                Dispose();
                return res;
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                throw;
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
            m_CommandManager.HandleCommandClick(cmdId);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int OnCommandEnable(string cmdId)
        {
            return m_CommandManager.HandleCommandEnable(cmdId);
        }

        public virtual bool OnConnect()
        {
            return true;
        }

        public virtual bool OnDisconnect()
        {
            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_CommandManager.Dispose();
            }

            if (m_Application != null)
            {
                if (Marshal.IsComObject(m_Application))
                {
                    Marshal.ReleaseComObject(m_Application);
                }
            }

            m_Application = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public IXPropertyPage<TData> CreatePage<TData>()
        {
            return CreatePropertyManagerPage<TData>(typeof(TData));
        }

        public SwPropertyManagerPage<TData> CreatePropertyManagerPage<TData, THandler>()
            where THandler : SwPropertyManagerPageHandler, new()
        {
            return CreatePropertyManagerPage<TData>(typeof(THandler));
        }

        private SwPropertyManagerPage<TData> CreatePropertyManagerPage<TData>(Type handlerType)
        {
            return new SwPropertyManagerPage<TData>(m_Application.Application, m_Logger, handlerType);
        }

        public IXCustomFeatureEditor<TData, TPage> CreateCustomFeatureEditor<TData, TPage>(
            Type defType,
            DataConverterDelegate<TPage, TData> pageToDataConv,
            DataConverterDelegate<TData, TPage> dataToPageConv,
            CreateGeometryDelegate<TData> geomCreator)
            where TData : class, new()
            where TPage : class, new()
        {
            return new SwMacroFeatureEditor<TData, TPage>(
                Application, this, defType, new MacroFeatureParametersParser(),
                pageToDataConv, dataToPageConv, geomCreator);
        }
    }
}
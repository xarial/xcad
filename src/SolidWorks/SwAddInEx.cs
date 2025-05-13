﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Delegates;
using Xarial.XCad.Documents;
using Xarial.XCad.Extensions;
using Xarial.XCad.Extensions.Attributes;
using Xarial.XCad.Extensions.Delegates;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Features.CustomFeature.Delegates;
using Xarial.XCad.SolidWorks.Attributes;
using Xarial.XCad.SolidWorks.Base;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature;
using Xarial.XCad.SolidWorks.Features.CustomFeature.Toolkit;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI;
using Xarial.XCad.SolidWorks.UI.Commands;
using Xarial.XCad.SolidWorks.UI.Commands.Exceptions;
using Xarial.XCad.SolidWorks.UI.Commands.Toolkit.Enums;
using Xarial.XCad.SolidWorks.UI.Commands.Toolkit.Structures;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.SolidWorks.UI.Toolkit;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Delegates;
using Xarial.XCad.UI.TaskPane;
using Xarial.XCad.Utils.Diagnostics;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks
{
    public interface ISwAddInEx : IXExtension
    {
        new ISwApplication Application { get; }
        new ISwCommandManager CommandManager { get; }

        new ISwPropertyManagerPage<TData> CreatePage<TData>(CreateDynamicControlsDelegate createDynCtrlHandler = null);

        ISwPropertyManagerPage<TData> CreatePage<TData, THandler>(CreateDynamicControlsDelegate createDynCtrlHandler = null)
                where THandler : SwPropertyManagerPageHandler, new();

        ISwModelViewTab<TControl> CreateDocumentTab<TControl>(ISwDocument doc);

        new ISwPopupWindow<TWindow> CreatePopupWindow<TWindow>(TWindow window);

        ISwTaskPane<TControl> CreateTaskPane<TControl>();

        new ISwTaskPane<TControl> CreateTaskPane<TControl>(TaskPaneSpec spec);

        ISwFeatureMgrTab<TControl> CreateFeatureManagerTab<TControl>(ISwDocument doc);
    }

    /// <inheritdoc/>
    [ComVisible(true)]
    public abstract class SwAddInEx : ISwAddInEx, ISwAddin, IXServiceConsumer, IDisposable
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

        public event ExtensionConnectDelegate Connect;
        public event ExtensionDisconnectDelegate Disconnect;
        public event ConfigureServicesDelegate ConfigureServices;
        public event ExtensionStartupCompletedDelegate StartupCompleted;

        IXApplication IXExtension.Application => Application;
        IXCommandManager IXExtension.CommandManager => CommandManager;
        IXCustomPanel<TControl> IXExtension.CreateDocumentTab<TControl>(IXDocument doc)
            => CreateDocumentTab<TControl>((SwDocument)doc);
        IXPopupWindow<TWindow> IXExtension.CreatePopupWindow<TWindow>(TWindow window)
            => CreatePopupWindow<TWindow>(window);
        IXTaskPane<TControl> IXExtension.CreateTaskPane<TControl>(TaskPaneSpec spec)
            => CreateTaskPane<TControl>(spec);
        IXCustomPanel<TControl> IXExtension.CreateFeatureManagerTab<TControl>(IXDocument doc)
            => CreateFeatureManagerTab<TControl>((SwDocument)doc);

        public ISwApplication Application => m_Application;

        private SwApplication m_Application;

        public ISwCommandManager CommandManager => m_CommandManager;

        private SwCommandManager m_CommandManager;

        /// <summary>
        /// Add-ins cookie (id)
        /// </summary>
        protected int AddInId { get; private set; }

        public IXLogger Logger { get; private set; }

        private readonly List<IDisposable> m_Disposables;

        protected IServiceProvider m_SvcProvider;

        public SwAddInEx()
        {
            m_Disposables = new List<IDisposable>();
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ConnectToSW(object ThisSW, int cookie)
        {
            m_IsDisposed = false;

            try
            {
                Validate();

                var app = ThisSW as ISldWorks;
                AddInId = cookie;

                if (app.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2015))
                {
                    app.SetAddinCallbackInfo2(0, this, AddInId);
                }
                else
                {
                    app.SetAddinCallbackInfo(0, this, AddInId);
                }

                m_Application = new SwApplication(app, OnStartupCompleted);

                var svcCollection = GetServiceCollection(m_Application);

                OnConfigureServices(svcCollection);

                m_SvcProvider = svcCollection.CreateProvider();

                m_Application.Init(m_SvcProvider);

                Logger = m_SvcProvider.GetService<IXLogger>();

                Logger.Log("Loading add-in", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);

                SwMacroFeatureDefinition.Application = m_Application;

                m_CommandManager = new SwCommandManager(Application, AddInId, m_SvcProvider);

                OnConnect();

                m_CommandManager.TryBuildCommandTabs();

                return true;
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return false;
            }
        }

        protected virtual void Validate()
        {
            if (this.GetType().TryGetAttribute<PartnerProductAttribute>(out _))
            {
                throw new Exception($"'{nameof(PartnerProductAttribute)}' must be used with {nameof(SwPartnerAddInEx)}");
            }
        }

        protected virtual void HandleException(Exception ex)
        {
            var logger = Logger ?? CreateDefaultLogger();
            logger.Log(ex);
        }

        private void OnStartupCompleted(SwApplication app)
        {
            try
            {
                StartupCompleted?.Invoke(this);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private IXServiceCollection GetServiceCollection(SwApplication app)
        {
            var svcCollection = CreateServiceCollection();

            app.LoadServices(svcCollection);

            svcCollection.Add<IXLogger>(CreateDefaultLogger, ServiceLifetimeScope_e.Singleton);
            svcCollection.Add<IIconsCreator, BaseIconsCreator>(ServiceLifetimeScope_e.Singleton);
            svcCollection.Add<IPropertyPageHandlerProvider, DataModelPropertyPageHandlerProvider>(ServiceLifetimeScope_e.Singleton);
            svcCollection.Add<IDragArrowHandlerProvider, NotSetDragArrowHandlerProvider>(ServiceLifetimeScope_e.Singleton);
            svcCollection.Add<ICalloutHandlerProvider, NotSetCalloutHandlerProvider>(ServiceLifetimeScope_e.Singleton);
            svcCollection.Add<ITriadHandlerProvider, NotSetTriadHandlerProvider>(ServiceLifetimeScope_e.Singleton);
            svcCollection.Add<IFeatureManagerTabControlProvider, FeatureManagerTabControlProvider>(ServiceLifetimeScope_e.Singleton);
            svcCollection.Add<ITaskPaneControlProvider, TaskPaneControlProvider>(ServiceLifetimeScope_e.Singleton);
            svcCollection.Add<IModelViewControlProvider, ModelViewControlProvider>(ServiceLifetimeScope_e.Singleton);
            svcCollection.Add<ICommandGroupTabConfigurer, DefaultCommandGroupTabConfigurer>(ServiceLifetimeScope_e.Singleton);

            return svcCollection;
        }

        protected IXLogger CreateDefaultLogger()
        {
            var addInType = this.GetType();
            var title = GetRegistrationHelper(addInType).GetTitle(addInType);
            return new TraceLogger($"XCad.AddIn.{title}");
        }

        protected virtual IXServiceCollection CreateServiceCollection() => new ServiceCollection();

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool DisconnectFromSW()
        {
            Logger.Log("Unloading add-in", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);

            try
            {
                OnDisconnect();
                Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
        }

        private bool m_IsDisposed;

        public void Dispose()
        {
            if (!m_IsDisposed)
            {
                Dispose(true);
                m_IsDisposed = true;
            }
        }

        /// <summary>
        /// Command click callback
        /// </summary>
        /// <param name="cmdId">Command tag</param>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnCommandClick(string cmdId)
        {
            try
            {
                m_CommandManager.HandleCommandClick(cmdId);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int OnCommandEnable(string cmdId)
        {
            try
            {
                return m_CommandManager.HandleCommandEnable(cmdId);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return (int)CommandItemEnableState_e.DeselectDisable;
            }
        }

        public virtual void OnConnect()
        {
            Connect?.Invoke(this);
        }

        public virtual void OnDisconnect()
        {
            Disconnect?.Invoke(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var dispCtrl in m_Disposables.ToArray())
                {
                    try
                    {
                        dispCtrl.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }
                }

                try
                {
                    CommandManager.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }

                try
                {
                    m_Application.Release(false);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        IXPropertyPage<TData> IXExtension.CreatePage<TData>(CreateDynamicControlsDelegate createDynCtrlHandler)
            => CreatePropertyManagerPage<TData>(typeof(TData), createDynCtrlHandler);

        public ISwPropertyManagerPage<TData> CreatePage<TData>(CreateDynamicControlsDelegate createDynCtrlHandler = null)
            => CreatePropertyManagerPage<TData>(typeof(TData), createDynCtrlHandler);

        public ISwPropertyManagerPage<TData> CreatePage<TData, THandler>(CreateDynamicControlsDelegate createDynCtrlHandler = null)
            where THandler : SwPropertyManagerPageHandler, new()
            => CreatePropertyManagerPage<TData>(typeof(THandler), createDynCtrlHandler);

        private ISwPropertyManagerPage<TData> CreatePropertyManagerPage<TData>(Type handlerType,
            CreateDynamicControlsDelegate createDynCtrlHandler)
        {
            var handler = m_SvcProvider.GetService<IPropertyPageHandlerProvider>().CreateHandler(Application, handlerType);

            var page = new SwPropertyManagerPage<TData>(m_Application, m_SvcProvider, handler, createDynCtrlHandler);
            page.Disposed += OnItemDisposed;
            m_Disposables.Add(page);
            return page;
        }

        public ISwModelViewTab<TControl> CreateDocumentTab<TControl>(ISwDocument doc)
        {
            var tab = new SwModelViewTab<TControl>(
                new ModelViewTabCreator<TControl>(doc.Model.ModelViewManager, m_SvcProvider),
                doc.Model.ModelViewManager, (SwDocument)doc, Application, Logger);

            tab.InitControl();

            tab.Disposed += OnItemDisposed;

            m_Disposables.Add(tab);

            return tab;
        }

        public ISwPopupWindow<TWindow> CreatePopupWindow<TWindow>(TWindow window)
        {
            var parent = (IntPtr)Application.Sw.IFrameObject().GetHWnd();

            if (typeof(System.Windows.Window).IsAssignableFrom(typeof(TWindow)))
            {
                return new SwPopupWpfWindow<TWindow>(window, parent);
            }
            else if (typeof(Form).IsAssignableFrom(typeof(TWindow)))
            {
                return new SwPopupWinForm<TWindow>(window, parent);
            }
            else
            {
                throw new NotSupportedException($"Only {typeof(Form).FullName} or {typeof(System.Windows.Window).FullName} are supported");
            }
        }

        public ISwTaskPane<TControl> CreateTaskPane<TControl>() => CreateTaskPane<TControl>(new TaskPaneSpec());

        public ISwTaskPane<TControl> CreateTaskPane<TControl>(TaskPaneSpec spec)
        {
            if (spec == null)
            {
                spec = new TaskPaneSpec();
            }

            var taskPane = new SwTaskPane<TControl>(new TaskPaneTabCreator<TControl>(Application, m_SvcProvider, spec), Logger);
            taskPane.Disposed += OnItemDisposed;

            m_Disposables.Add(taskPane);

            return taskPane;
        }

        public ISwFeatureMgrTab<TControl> CreateFeatureManagerTab<TControl>(ISwDocument doc)
        {
            var tab = new SwFeatureMgrTab<TControl>(
                new FeatureManagerTabCreator<TControl>(doc.Model.ModelViewManager, m_SvcProvider),
                (SwDocument)doc, Application, Logger);

            tab.InitControl();
            tab.Disposed += OnItemDisposed;
            m_Disposables.Add(tab);

            return tab;
        }

        protected virtual void OnConfigureServices(IXServiceCollection svcCollection)
        {
            ConfigureServices?.Invoke(this, svcCollection);
        }

        private void OnItemDisposed(IAutoDisposable item)
        {
            item.Disposed -= OnItemDisposed;

            if (m_Disposables.Contains(item))
            {
                m_Disposables.Remove(item);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, "Disposable is not registered");
            }
        }

        public IXWorkUnit PreCreateWorkUnit() => new SwWorkUnit(m_Application);
    }

    /// <inheritdoc/>
    [ComVisible(true)]
    public abstract class SwPartnerAddInEx : SwAddInEx, ISwPEManager
    {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void IdentifyToSW(object classFactory)
        {
            if (this.GetType().TryGetAttribute<PartnerProductAttribute>(out var att))
            {
                try
                {
                    var res = (swPartnerEntitlementStatus_e)((ISwPEClassFactory)classFactory).SetPartnerKey(att.PartnerKey, out _);

                    if (res != swPartnerEntitlementStatus_e.swPESuccess)
                    {
                        throw new Exception($"Failed to register partner product: {res}");
                    }
                }
                catch (Exception ex)
                {
                    var logger = Logger ?? CreateDefaultLogger();
                    logger.Log(ex);
                }
            }
            else
            {
                throw new Exception($"Decorate the add-in class with '{typeof(PartnerProductAttribute).FullName}' to specify partner key");
            }
        }

        protected override void Validate()
        {
        }
    }

    public static class SwAddInExExtension
    {
        public static ISwModelViewTab<TControl> CreateDocumentTabWinForm<TControl>(this ISwAddInEx addIn, ISwDocument doc)
            where TControl : Control => addIn.CreateDocumentTab<TControl>(doc);

        public static ISwModelViewTab<TControl> CreateDocumentTabWpf<TControl>(this ISwAddInEx addIn, ISwDocument doc)
            where TControl : System.Windows.UIElement => addIn.CreateDocumentTab<TControl>(doc);

        public static ISwPopupWindow<TWindow> CreatePopupWpfWindow<TWindow>(this ISwAddInEx addIn)
            where TWindow : System.Windows.Window => (SwPopupWpfWindow<TWindow>)addIn.CreatePopupWindow<TWindow>((TWindow)Activator.CreateInstance(typeof(TWindow)));

        public static ISwPopupWindow<TWindow> CreatePopupWinForm<TWindow>(this ISwAddInEx addIn)
            where TWindow : Form => (SwPopupWinForm<TWindow>)addIn.CreatePopupWindow<TWindow>((TWindow)Activator.CreateInstance(typeof(TWindow)));

        public static ISwTaskPane<TControl> CreateTaskPaneWinForm<TControl>(this ISwAddInEx addIn, TaskPaneSpec spec = null)
            where TControl : Control => addIn.CreateTaskPane<TControl>(spec);

        public static ISwTaskPane<TControl> CreateTaskPaneWpf<TControl>(this ISwAddInEx addIn, TaskPaneSpec spec = null)
            where TControl : System.Windows.UIElement => addIn.CreateTaskPane<TControl>(spec);

        public static IXEnumTaskPane<TControl, TEnum> CreateTaskPaneWinForm<TControl, TEnum>(this ISwAddInEx addIn)
            where TControl : Control
            where TEnum : Enum => addIn.CreateTaskPane<TControl, TEnum>();

        public static IXEnumTaskPane<TControl, TEnum> CreateTaskPaneWpf<TControl, TEnum>(this ISwAddInEx addIn)
            where TControl : System.Windows.UIElement
            where TEnum : Enum => addIn.CreateTaskPane<TControl, TEnum>();

        public static ISwFeatureMgrTab<TControl> CreateFeatureManagerTabWpf<TControl>(this ISwAddInEx addIn, ISwDocument doc)
            where TControl : System.Windows.UIElement => addIn.CreateFeatureManagerTab<TControl>(doc);

        public static ISwFeatureMgrTab<TControl> CreateFeatureManagerTabWinForm<TControl>(this ISwAddInEx addIn, ISwDocument doc)
            where TControl : Control => addIn.CreateFeatureManagerTab<TControl>(doc);
    }
}
//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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
using Xarial.XCad.SolidWorks.Base;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature;
using Xarial.XCad.SolidWorks.Features.CustomFeature.Toolkit;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI;
using Xarial.XCad.SolidWorks.UI.Commands;
using Xarial.XCad.SolidWorks.UI.Commands.Exceptions;
using Xarial.XCad.SolidWorks.UI.Commands.Toolkit.Structures;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.SolidWorks.UI.Toolkit;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit;
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
        
        new ISwPopupWindow<TWindow> CreatePopupWindow<TWindow>();
        
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
        IXPopupWindow<TWindow> IXExtension.CreatePopupWindow<TWindow>()
            => CreatePopupWindow<TWindow>();
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

        private IServiceProvider m_SvcProvider;
        
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

                var svcCollection = GetServicesCollection();

                ConfigureServices?.Invoke(this, svcCollection);
                OnConfigureServices(svcCollection);

                m_SvcProvider = svcCollection.CreateProvider();

                Logger = m_SvcProvider.GetService<IXLogger>();

                m_Application.Init(svcCollection);

                Logger.Log("Loading add-in", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);

                SwMacroFeatureDefinition.Application = m_Application;

                m_CommandManager = new SwCommandManager(Application, AddInId, m_SvcProvider);

                Connect?.Invoke(this);
                OnConnect();

                m_CommandManager.TryBuildCommandTabs();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
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

        private IXServiceCollection GetServicesCollection()
        {
            var svcCollection = new ServiceCollection();

            var addInType = this.GetType();
            var title = GetRegistrationHelper(addInType).GetTitle(addInType);

            svcCollection.AddOrReplace<IXLogger>(() => new TraceLogger($"XCad.AddIn.{title}"));
            svcCollection.AddOrReplace<IIconsCreator, BaseIconsCreator>();
            svcCollection.AddOrReplace<IPropertyPageHandlerProvider, PropertyPageHandlerProvider>();
            svcCollection.AddOrReplace<IFeatureManagerTabControlProvider, FeatureManagerTabControlProvider>();
            svcCollection.AddOrReplace<ITaskPaneControlProvider, TaskPaneControlProvider>();
            svcCollection.AddOrReplace<IModelViewControlProvider, ModelViewControlProvider>();
            svcCollection.AddOrReplace<ICommandGroupTabConfigurer, DefaultCommandGroupTabConfigurer>();

            return svcCollection;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool DisconnectFromSW()
        {
            Logger.Log("Unloading add-in", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);

            try
            {
                Disconnect?.Invoke(this);
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
            m_CommandManager.HandleCommandClick(cmdId);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int OnCommandEnable(string cmdId)
        {
            return m_CommandManager.HandleCommandEnable(cmdId);
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
                    Application.Dispose();
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
            var handler = m_SvcProvider.GetService<IPropertyPageHandlerProvider>().CreateHandler(Application.Sw, handlerType);

            var page = new SwPropertyManagerPage<TData>(Application, m_SvcProvider, handler, createDynCtrlHandler);
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
        
        public ISwPopupWindow<TWindow> CreatePopupWindow<TWindow>() 
        {
            var parent = (IntPtr)Application.Sw.IFrameObject().GetHWnd();

            if (typeof(System.Windows.Window).IsAssignableFrom(typeof(TWindow)))
            {
                return new SwPopupWpfWindow<TWindow>((TWindow)Activator.CreateInstance(typeof(TWindow)), parent);
            }
            else if (typeof(Form).IsAssignableFrom(typeof(TWindow)))
            {
                return new SwPopupWinForm<TWindow>((TWindow)Activator.CreateInstance(typeof(TWindow)), parent);
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
        
        public virtual void OnConfigureServices(IXServiceCollection collection)
        {
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
    }

    public static class SwAddInExExtension 
    {
        public static ISwModelViewTab<TControl> CreateDocumentTabWinForm<TControl>(this ISwAddInEx addIn, ISwDocument doc)
            where TControl : Control => addIn.CreateDocumentTab<TControl>(doc);

        public static ISwModelViewTab<TControl> CreateDocumentTabWpf<TControl>(this ISwAddInEx addIn, ISwDocument doc)
            where TControl : System.Windows.UIElement => addIn.CreateDocumentTab<TControl>(doc);

        public static ISwPopupWindow<TWindow> CreatePopupWpfWindow<TWindow>(this ISwAddInEx addIn)
            where TWindow : System.Windows.Window => (SwPopupWpfWindow<TWindow>)addIn.CreatePopupWindow<TWindow>();

        public static ISwPopupWindow<TWindow> CreatePopupWinForm<TWindow>(this ISwAddInEx addIn)
            where TWindow : Form => (SwPopupWinForm<TWindow>)addIn.CreatePopupWindow<TWindow>();

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
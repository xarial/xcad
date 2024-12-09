//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Windows;
using Xarial.XCad.Utils.Diagnostics;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using Xarial.XCad.SolidWorks.Exceptions;
using Xarial.XCad.Base;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Drawing;
using Xarial.XCad.Toolkit;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Services;
using Xarial.XCad.Enums;
using Xarial.XCad.Delegates;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.UI;
using Xarial.XCad.SolidWorks.UI;
using Xarial.XCad.Reflection;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Data;

namespace Xarial.XCad.SolidWorks
{
    public interface ISwApplication : IXApplication, IDisposable
    {
        ISldWorks Sw { get; }
        new ISwVersion Version { get; set; }

        IXServiceCollection CustomServices { get; set; }

        new ISwDocumentCollection Documents { get; }
        new ISwMemoryGeometryBuilder MemoryGeometryBuilder { get; }
        new ISwMacro OpenMacro(string path);

        TObj CreateObjectFromDispatch<TObj>(object disp, ISwDocument doc)
            where TObj : ISwObject;
    }

    public interface ISwApplicationOptions : ISwOptions, IXApplicationOptions 
    {
    }

    internal class SwApplicationOptions : SwOptions, ISwApplicationOptions 
    {
        private readonly SwApplication m_App;

        internal SwApplicationOptions(SwApplication app) 
        {
            m_App = app;
            Drawings = new SwDrawingsApplicationOptions(app);
        }

        public IXDrawingsApplicationOptions Drawings { get; }
    }

    internal class SwDrawingsApplicationOptions : IXDrawingsApplicationOptions
    {
        private readonly SwApplication m_App;

        public SwDrawingsApplicationOptions(SwApplication app)
        {
            m_App = app;
        }

        public bool AutomaticallyScaleNewDrawingViews
        {
            get => m_App.Sw.GetUserPreferenceToggle((int)swUserPreferenceToggle_e.swAutomaticScaling3ViewDrawings);
            set => m_App.Sw.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swAutomaticScaling3ViewDrawings, value);
        }
    }

    /// <inheritdoc/>
    internal class SwApplication : ISwApplication, IXServiceConsumer
    {
        #region WinApi
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        #endregion

        IXDocumentRepository IXApplication.Documents => Documents;
        IXMacro IXApplication.OpenMacro(string path) => OpenMacro(path);
        IXMemoryGeometryBuilder IXApplication.MemoryGeometryBuilder => MemoryGeometryBuilder;
        IXVersion IXApplication.Version
        {
            get => Version;
            set => Version = (ISwVersion)value;
        }
        IXMaterialsDatabaseRepository IXApplication.MaterialDatabases => MaterialDatabases;

        public event ApplicationStartingDelegate Starting;
        public event ConfigureServicesDelegate ConfigureServices;

        public event ApplicationIdleDelegate Idle
        {
            add
            {
                if (m_IdleDelegate == null)
                {
                    ((SldWorks)Sw).OnIdleNotify += OnIdleNotify;
                }

                m_IdleDelegate += value;
            }
            remove
            {
                m_IdleDelegate -= value;

                if (m_IdleDelegate == null)
                {
                    ((SldWorks)Sw).OnIdleNotify -= OnIdleNotify;
                }
            }
        }

        private int OnIdleNotify()
        {
            m_IdleDelegate?.Invoke(this);

            return HResult.S_OK;
        }

        private IXServiceCollection m_CustomServices;

        public ISldWorks Sw => m_Creator.Element;

        public ISwVersion Version
        {
            get
            {
                if (IsCommitted)
                {
                    var major = Sw.GetVersion(out var sp, out var spRev);
                    var minor = sp > 0 ? sp : 0;//pre-release version will have a negative SP
                    var build = spRev > 0 ? spRev : 0;

                    var version = VersionMapper.FromApplicationRevision(major);
                    var dispName = VersionMapper.GetVersionName(version);

                    return new SwVersion(new Version(major, minor, build), version, sp, spRev, dispName);
                }
                else
                {
                    return m_Creator.CachedProperties.Get<SwVersion>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    throw new Exception("Version cannot be changed after the application is committed");
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        private SwDocumentCollection m_Documents;

        public ISwDocumentCollection Documents => m_Documents;

        public IntPtr WindowHandle => new IntPtr(Sw.IFrameObject().GetHWndx64());

        public Process Process => Process.GetProcessById(Sw.GetProcessID());

        public Rectangle WindowRectangle => new Rectangle(Sw.FrameLeft, Sw.FrameTop, Sw.FrameWidth, Sw.FrameHeight);

        public ISwMemoryGeometryBuilder MemoryGeometryBuilder { get; private set; }

        public IXApplicationOptions Options { get; }

        public bool IsCommitted => m_Creator.IsCreated;

        public ApplicationState_e State
        {
            get
            {
                if (IsCommitted)
                {
                    return GetApplicationState();
                }
                else
                {
                    return m_Creator.CachedProperties.Get<ApplicationState_e>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    var curState = GetApplicationState();

                    if (curState == value)
                    {
                        //do nothing
                    }
                    else if (((int)curState - (int)value) == (int)ApplicationState_e.Hidden)
                    {
                        Sw.Visible = true;
                    }
                    else if ((int)value - ((int)curState) == (int)ApplicationState_e.Hidden)
                    {
                        Sw.Visible = false;
                    }
                    else
                    {
                        throw new Exception("Only visibility can changed after the application is started");
                    }
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public IXServiceCollection CustomServices
        {
            get => m_CustomServices;
            set
            {
                if (!IsCommitted)
                {
                    m_CustomServices = value;
                }
                else
                {
                    throw new Exception("Services can only be set before committing");
                }
            }
        }

        internal IXLogger Logger { get; private set; }

        internal IServiceProvider Services { get; private set; }

        public bool IsAlive
        {
            get
            {
                try
                {
                    if (Process == null || Process.HasExited || !Process.Responding)
                    {
                        return false;
                    }
                    else
                    {
                        var testCall = Sw.RevisionNumber();
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        private bool m_IsDisposed;
        private bool m_IsClosed;

        private bool m_IsInitialized;

        private bool m_HideOnStartup;

        private bool m_IsStartupNotified;

        private readonly IElementCreator<ISldWorks> m_Creator;

        private ApplicationIdleDelegate m_IdleDelegate;

        private readonly Action<SwApplication> m_StartupCompletedCallback;

        internal GlobalTagsRegistry TagsRegistry { get; }

        public SwMaterialsDatabaseRepository MaterialDatabases { get; private set; }

        internal ISwVersionMapper VersionMapper { get; private set; }

        internal SwApplication(ISldWorks app, IXServiceCollection customServices) 
            : this(default)
        {
            if (app == null) 
            {
                throw new ArgumentNullException(nameof(app));
            }

            m_Creator = new ElementCreator<ISldWorks>(CreateInstance, app, true);
            WatchStartupCompleted((SldWorks)app);

            customServices = customServices ?? new ServiceCollection();

            LoadServices(customServices);
            Init(customServices.CreateProvider());
        }

        /// <summary>
        /// Only to be used within SwAddInEx
        /// </summary>
        internal SwApplication(Action<SwApplication> startupCompletedCallback, Func<ISldWorks> swProvider,
            IXServiceCollection customServices) 
            : this(startupCompletedCallback)
        {
            m_CustomServices = customServices ?? new ServiceCollection();

            m_Creator = new ElementCreator<ISldWorks>(
                c => swProvider.Invoke(),
                (s, c) => WatchStartupCompleted((SldWorks)s),
                null, false);
        }

        /// <Remarks>
        /// Used for <see cref="SwApplicationFactory.PreCreate"/>
        /// </Remarks>
        internal SwApplication() : this(default)
        {
            m_Creator = new ElementCreator<ISldWorks>(CreateInstance, null, false);
            m_Creator.CachedProperties.Set(new ServiceCollection(), nameof(CustomServices));
        }

        private SwApplication(Action<SwApplication> startupCompletedCallback)
        {
            m_IsStartupNotified = false;
            m_StartupCompletedCallback = startupCompletedCallback;

            TagsRegistry = new GlobalTagsRegistry();

            Options = new SwApplicationOptions(this);
        }

        private void LoadServices(IXServiceCollection customServices)
        {
            if (!m_IsInitialized)
            {
                m_CustomServices = customServices;

                customServices.Add<IXLogger>(() => new TraceLogger("xCAD.SwApplication"), ServiceLifetimeScope_e.Singleton, false);
                customServices.Add<IMemoryGeometryBuilderDocumentProvider>(() => new DefaultMemoryGeometryBuilderDocumentProvider(this), ServiceLifetimeScope_e.Singleton, false);
                customServices.Add<IFilePathResolver>(() => new SwFilePathResolverNoSearchFolders(this), ServiceLifetimeScope_e.Singleton, false);//TODO: there is some issue with recursive search of folders in search locations - do a test to validate
                customServices.Add<IMemoryGeometryBuilderToleranceProvider, DefaultMemoryGeometryBuilderToleranceProvider>(ServiceLifetimeScope_e.Singleton, false);
                customServices.Add<IIconsCreator, BaseIconsCreator>(ServiceLifetimeScope_e.Singleton, false);
                customServices.Add<ISwVersionMapper, SwVersionMapper>(ServiceLifetimeScope_e.Singleton, false);
                customServices.Add<IMacroFeatureTypeProvider, ComMacroFeatureTypeProvider>(ServiceLifetimeScope_e.Singleton);
                customServices.Add<IInterferencesProvider, InterferencesProvider>(ServiceLifetimeScope_e.Singleton);
                customServices.Add<ICustomGraphicsContextProvider, OglCustomGraphicsContextProvider>(ServiceLifetimeScope_e.Singleton);

                ConfigureServices?.Invoke(this, customServices);
            }
            else
            {
                Debug.Assert(false, "App has been already initialized. Must be only once");
            }
        }

        private void Init(IServiceProvider svcProvider)
        {
            if (!m_IsInitialized)
            {
                m_IsInitialized = true;

                Services = svcProvider;
                Logger = Services.GetService<IXLogger>();

                VersionMapper = Services.GetService<ISwVersionMapper>();

                m_Documents = new SwDocumentCollection(this, Logger);

                MaterialDatabases = new SwMaterialsDatabaseRepository(this);

                MemoryGeometryBuilder = new SwMemoryGeometryBuilder(this,
                    Services.GetService<IMemoryGeometryBuilderDocumentProvider>(),
                    Services.GetService<IMemoryGeometryBuilderToleranceProvider>());
            }
            else 
            {
                Debug.Assert(false, "App has been already initialized. Must be only once");
            }
        }

        public MessageBoxResult_e ShowMessageBox(string msg, MessageBoxIcon_e icon = MessageBoxIcon_e.Info, MessageBoxButtons_e buttons = MessageBoxButtons_e.Ok)
        {
            swMessageBoxBtn_e swBtn = 0;
            swMessageBoxIcon_e swIcon = 0;

            switch (icon)
            {
                case MessageBoxIcon_e.Info:
                    swIcon = swMessageBoxIcon_e.swMbInformation;
                    break;

                case MessageBoxIcon_e.Question:
                    swIcon = swMessageBoxIcon_e.swMbQuestion;
                    break;

                case MessageBoxIcon_e.Error:
                    swIcon = swMessageBoxIcon_e.swMbStop;
                    break;

                case MessageBoxIcon_e.Warning:
                    swIcon = swMessageBoxIcon_e.swMbWarning;
                    break;
            }

            switch (buttons)
            {
                case MessageBoxButtons_e.Ok:
                    swBtn = swMessageBoxBtn_e.swMbOk;
                    break;

                case MessageBoxButtons_e.YesNo:
                    swBtn = swMessageBoxBtn_e.swMbYesNo;
                    break;

                case MessageBoxButtons_e.OkCancel:
                    swBtn = swMessageBoxBtn_e.swMbOkCancel;
                    break;

                case MessageBoxButtons_e.YesNoCancel:
                    swBtn = swMessageBoxBtn_e.swMbYesNoCancel;
                    break;
            }

            var swRes = (swMessageBoxResult_e)Sw.SendMsgToUser2(msg, (int)swIcon, (int)swBtn);

            switch (swRes)
            {
                case swMessageBoxResult_e.swMbHitOk:
                    return MessageBoxResult_e.Ok;

                case swMessageBoxResult_e.swMbHitCancel:
                    return MessageBoxResult_e.Cancel;

                case swMessageBoxResult_e.swMbHitYes:
                    return MessageBoxResult_e.Yes;

                case swMessageBoxResult_e.swMbHitNo:
                    return MessageBoxResult_e.No;

                default:
                    return 0;
            }
        }

        public ISwMacro OpenMacro(string path)
        {
            const string VSTA_FILE_EXT = ".dll";
            const string VBA_FILE_EXT = ".swp";
            const string BASIC_EXT = ".swb";

            var ext = Path.GetExtension(path);

            switch (ext.ToLower()) 
            {
                case VSTA_FILE_EXT:
                    return new SwVstaMacro(this, path);

                case VBA_FILE_EXT:
                case BASIC_EXT:
                    return new SwVbaMacro(Sw, path);

                default:
                    throw new NotSupportedException("Specified file is not a SOLIDWORKS macro");
            }
        }

        public void Commit(CancellationToken cancellationToken)
        {
            m_Creator.Create(cancellationToken);

            var customServices = CustomServices ?? new ServiceCollection();
            LoadServices(customServices);
            Init(customServices.CreateProvider());
        }

        private ISldWorks CreateInstance(CancellationToken cancellationToken)
        {
            m_HideOnStartup = State.HasFlag(ApplicationState_e.Hidden);

            using (var appStarter = new SwApplicationStarter(State, Version)) 
            {
                var logger = Logger ?? new TraceLogger("xCAD.SwApplication");

                var app = appStarter.Start(p => Starting?.Invoke(this, p), logger, cancellationToken);
                WatchStartupCompleted((SldWorks)app);
                return app;
            }
        }

        private void WatchStartupCompleted(SldWorks sw) 
        {
            sw.OnIdleNotify += OnLoadFirstIdleNotify;
        }

        private int OnLoadFirstIdleNotify()
        {
            Debug.Assert(!m_IsStartupNotified, "This event shoud only be fired once");
            
            if (!m_IsStartupNotified)
            {
                if (Sw?.StartupProcessCompleted == true)
                {
                    if (m_HideOnStartup)
                    {
                        const int HIDE = 0;
                        ShowWindow(new IntPtr(Sw.IFrameObject().GetHWnd()), HIDE);

                        Sw.Visible = false;
                    }

                    m_IsStartupNotified = true;

                    m_StartupCompletedCallback?.Invoke(this);

                    if (Sw != null)
                    {
                        (Sw as SldWorks).OnIdleNotify -= OnLoadFirstIdleNotify;
                    }
                }
            }
            else
            {
                (Sw as SldWorks).OnIdleNotify -= OnLoadFirstIdleNotify;
            }

            return HResult.S_OK;
        }

        private ApplicationState_e GetApplicationState() 
        {
            //TODO: find the state
            return ApplicationState_e.Default;
        }

        public IXProgress CreateProgress()
        {
            if (Sw.GetUserProgressBar(out UserProgressBar prgBar))
            {
                return new SwProgress(prgBar);
            }
            else 
            {
                throw new Exception("Failed to create progress");
            }
        }

        public void ShowTooltip(ITooltipSpec spec)
        {
            IXImage icon = null;

            spec.GetType().TryGetAttribute<IconAttribute>(a => icon = a.Icon);

            var bmpType = icon != null ? swBitMaps.swBitMapUserDefined : swBitMaps.swBitMapNone;

            using (var bmp = CreateTooltipIcon(icon)) 
            {
                Sw.HideBubbleTooltip();

                Sw.ShowBubbleTooltipAt2(spec.Position.X, spec.Position.Y, (int)spec.ArrowPosition,
                            spec.Title, spec.Message, (int)bmpType,
                            bmp?.FilePaths.First(), "", 0, (int)swLinkString.swLinkStringNone, "", "");
            }
        }

        private IImageCollection CreateTooltipIcon(IXImage icon) 
        {
            if (icon != null)
            {
                var iconsCreator = Services.GetService<IIconsCreator>();

                return iconsCreator.ConvertIcon(new TooltipIcon(icon));
            }
            else 
            {
                return null;
            }
        }

        public TObj CreateObjectFromDispatch<TObj>(object disp, ISwDocument doc)
            where TObj : ISwObject
            => SwObjectFactory.FromDispatch<TObj>(disp, (SwDocument)doc, this);

        public IXObjectTracker CreateObjectTracker(string name) 
            => new SwObjectTracker(this, name);

        internal void Release(bool close)
        {
            if (!m_IsDisposed)
            {
                m_IsDisposed = true;

                if (Services is IDisposable)
                {
                    ((IDisposable)Services).Dispose();
                }

                try
                {
                    m_Documents.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }

                TagsRegistry.Dispose();

                if (close)
                {
                    if (!m_IsClosed)
                    {
                        Close();
                    }
                }

                if (Sw != null)
                {
                    if (Marshal.IsComObject(Sw))
                    {
                        Marshal.ReleaseComObject(Sw);
                    }
                }
            }
        }

        public void Dispose() => Release(true);

        public void Close()
        {
            if (!m_IsClosed)
            {
                m_IsClosed = true;
                Sw.ExitApp();
                Dispose();
            }
        }
    }

    /// <summary>
    /// Additional methods of <see cref="ISwApplication"/>
    /// </summary>
    public static class SwApplicationExtension 
    {
        /// <summary>
        /// Checks if the current version of the SOLIDWORKS applicating equals or newver than the specified version
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="version">Major version</param>
        /// <param name="servicePack">Service pack</param>
        /// <param name="servicePackRev">Revision</param>
        /// <returns>True if current version is newer or equal</returns>
        /// <remarks>Use this method for forward compatibility</remarks>
        public static bool IsVersionNewerOrEqual(this ISwApplication app, SwVersion_e version, 
            int? servicePack = null, int? servicePackRev = null) 
        {
            return app.Sw.IsVersionNewerOrEqual(version, servicePack, servicePackRev);
        }

        /// <summary>
        /// Checks if currently running application is in-process application
        /// </summary>
        /// <param name="app">Application</param>
        /// <returns>True if in process</returns>
        /// <remarks>This method also checks the UI thread</remarks>
        public static bool IsInProcess(this ISwApplication app) 
        {
            if (Process.GetCurrentProcess().Id == app.Process.Id)
            {
                return Thread.CurrentThread.ManagedThreadId == 1;
            }
            else 
            {
                return false;
            }
        }
    }
}
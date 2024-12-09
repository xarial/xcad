//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Inventor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Delegates;
using Xarial.XCad.Documents;
using Xarial.XCad.Enums;
using Xarial.XCad.Geometry;
using Xarial.XCad.Inventor.Documents;
using Xarial.XCad.Inventor.Enums;
using Xarial.XCad.Inventor.Services;
using Xarial.XCad.Inventor.Utils;
using Xarial.XCad.Services;
using Xarial.XCad.Toolkit;
using Xarial.XCad.Utils.Diagnostics;

namespace Xarial.XCad.Inventor
{
    public interface IAiApplication : IXApplication
    {
        Application Application { get; }
        
        new IAiVersion Version { get; set; }

        StartApplicationConnectStrategy_e StartApplicationConnectStrategy { get; set; }

        IXServiceCollection CustomServices { get; set; }
    }

    internal class AiApplication : IAiApplication, IXServiceConsumer
    {
        public IXMaterialsDatabaseRepository MaterialDatabases => throw new NotSupportedException();

        IXVersion IXApplication.Version
        {
            get => Version;
            set => Version = (IAiVersion)value;
        }

        public Application Application => m_Creator.Element;
        
        public ApplicationState_e State 
        {
            get 
            {
                if (IsCommitted)
                {
                    return ApplicationState_e.Default;
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
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public bool IsAlive => throw new NotImplementedException();
        public Rectangle WindowRectangle => throw new NotImplementedException();
        public IntPtr WindowHandle => new IntPtr(Application.MainFrameHWND);
        public Process Process 
        {
            get 
            {
                WinAPI.GetWindowThreadProcessId(WindowHandle, out var prcId);
                return Process.GetProcessById(prcId);
            }
        }
        public IXApplicationOptions Options => throw new NotImplementedException();

        public IXDocumentRepository Documents => m_Documents;

        public IXMemoryGeometryBuilder MemoryGeometryBuilder => throw new NotImplementedException();

        public bool IsCommitted => m_Creator.IsCreated;

        public event ApplicationStartingDelegate Starting;
        public event ApplicationIdleDelegate Idle;
        public event ConfigureServicesDelegate ConfigureServices;

        public void Commit(CancellationToken cancellationToken)
        {
            m_Creator.Create(cancellationToken);

            var customServices = CustomServices ?? new ServiceCollection();
            LoadServices(customServices);
            Init(customServices.CreateProvider());
        }

        public IXObjectTracker CreateObjectTracker(string name)
        {
            throw new NotImplementedException();
        }

        public IXProgress CreateProgress()
        {
            throw new NotImplementedException();
        }

        public IXMacro OpenMacro(string path)
        {
            throw new NotImplementedException();
        }

        public MessageBoxResult_e ShowMessageBox(string msg, MessageBoxIcon_e icon = MessageBoxIcon_e.Info, MessageBoxButtons_e buttons = MessageBoxButtons_e.Ok)
        {
            throw new NotImplementedException();
        }

        public void ShowTooltip(ITooltipSpec spec)
        {
            throw new NotImplementedException();
        }

        internal IXLogger Logger { get; private set; }
        internal IServiceProvider Services { get; private set; }

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

        public IAiVersion Version 
        {
            get 
            {
                if (IsCommitted)
                {
                    var softwareVersion = Application.SoftwareVersion;

                    return new AiVersion(softwareVersion, VersionMapper.FromApplicationRevision(softwareVersion.Major));
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<IAiVersion>();
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

        public StartApplicationConnectStrategy_e StartApplicationConnectStrategy
        {
            get
            {
                if (IsCommitted)
                {
                    throw new Exception("This property is only used to start new instance of the application");
                }
                else
                {
                    return m_Creator.CachedProperties.Get<StartApplicationConnectStrategy_e>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    throw new Exception("This property is only used to start new instance of the application");
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        internal IAiVersionMapper VersionMapper { get; private set; }

        private readonly IElementCreator<Application> m_Creator;
        private readonly Action<AiApplication> m_StartupCompletedCallback;

        private bool m_IsInitialized;
        private bool m_IsDisposed;
        private bool m_IsClosed;

        private IXServiceCollection m_CustomServices;

        private AiDocumentsCollection m_Documents;

        internal AiApplication(Application app, IXServiceCollection customServices)
            : this(app, default(Action<AiApplication>))
        {
            customServices = customServices ?? new ServiceCollection();

            LoadServices(customServices);
            Init(customServices.CreateProvider());
        }

        /// <summary>
        /// Only to be used within SwAddInEx
        /// </summary>
        internal AiApplication(Application app, Action<AiApplication> startupCompletedCallback)
        {
            m_StartupCompletedCallback = startupCompletedCallback;

            m_Creator = new ElementCreator<Application>(CreateInstance, app, true);
            WatchStartupCompleted(app);
        }

        internal AiApplication()
        {
            m_Creator = new ElementCreator<Application>(CreateInstance, null, false);

            m_Creator.CachedProperties.Set(new ServiceCollection(), nameof(CustomServices));
        }

        internal void LoadServices(IXServiceCollection customServices)
        {
            if (!m_IsInitialized)
            {
                m_CustomServices = customServices;

                customServices.Add<IXLogger>(() => new TraceLogger("xCAD.AiApplication"), ServiceLifetimeScope_e.Singleton, false);
                customServices.Add<IAiVersionMapper, AiVersionMapper>(ServiceLifetimeScope_e.Singleton, false);
                
                ConfigureServices?.Invoke(this, customServices);
            }
            else
            {
                Debug.Assert(false, "App has been already initialized. Must be only once");
            }
        }

        internal void Init(IServiceProvider svcProvider)
        {
            if (!m_IsInitialized)
            {
                m_IsInitialized = true;

                Services = svcProvider;
                Logger = Services.GetService<IXLogger>();
                VersionMapper = Services.GetService<IAiVersionMapper>();

                m_Documents = new AiDocumentsCollection(this, Logger);
            }
            else
            {
                Debug.Assert(false, "App has been already initialized. Must be only once");
            }
        }

        public void Close()
        {
            if (!m_IsClosed)
            {
                m_IsClosed = true;
                Application.Quit();
                Dispose();
            }
        }

        public void Dispose()
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

                if (!m_IsClosed)
                {
                    Close();
                }

                if (Application != null)
                {
                    if (Marshal.IsComObject(Application))
                    {
                        Marshal.ReleaseComObject(Application);
                    }
                }
            }
        }

        private Application CreateInstance(CancellationToken cancellationToken)
        {
            var logger = Logger ?? new TraceLogger("xCAD.AiApplication");

            using (var appStarter = new InventorApplicationStarter(State, Version,
                StartApplicationConnectStrategy, logger))
            {
                var app = appStarter.Start(p => Starting?.Invoke(this, p), cancellationToken);

                WatchStartupCompleted(app);

                return app;
            }
        }

        private void WatchStartupCompleted(Application app)
        {
            if (app.Ready)
            {
                HandleStartupCompleted(app);
            }
            else
            {
                app.ApplicationEvents.OnReady += OnApplicationReady;
            }
        }

        private void OnApplicationReady(EventTimingEnum beforeOrAfter, NameValueMap context, out HandlingCodeEnum handlingCode)
        {
            handlingCode = HandlingCodeEnum.kEventHandled;

            HandleStartupCompleted(Application);

            Application.ApplicationEvents.OnReady -= OnApplicationReady;
        }

        private void HandleStartupCompleted(Application app)
        {
            if (m_Creator.CachedProperties.Get<ApplicationState_e>(nameof(State)).HasFlag(ApplicationState_e.Hidden))
            {
                app.Visible = false;
            }

            m_StartupCompletedCallback?.Invoke(this);
        }
    }
}

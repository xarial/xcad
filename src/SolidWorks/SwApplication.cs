//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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
    }

    /// <inheritdoc/>
    internal class SwApplication : ISwApplication, IXServiceConsumer
    {
        #region WinApi
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        #endregion

        public event ConfigureServicesDelegate ConfigureServices;

        internal event Action<SwApplication> FirstStartupCompleted;

        IXDocumentRepository IXApplication.Documents => Documents;
        IXMacro IXApplication.OpenMacro(string path) => OpenMacro(path);
        IXGeometryBuilder IXApplication.MemoryGeometryBuilder => MemoryGeometryBuilder;
        IXVersion IXApplication.Version 
        {
            get => Version;
            set => Version = (ISwVersion)value;
        }

        private IXServiceCollection m_CustomServices;

        public ISldWorks Sw => m_Creator.Element;

        public ISwVersion Version 
        {
            get 
            {
                if (IsCommitted)
                {
                    return new SwVersion(Sw.GetVersion());
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

        private Lazy<ISwDocumentCollection> m_DocumentsLazy;

        public ISwDocumentCollection Documents => m_DocumentsLazy.Value;
        
        public IntPtr WindowHandle => new IntPtr(Sw.IFrameObject().GetHWndx64());

        public Process Process => Process.GetProcessById(Sw.GetProcessID());

        public Rectangle WindowRectangle => new Rectangle(Sw.FrameLeft, Sw.FrameTop, Sw.FrameWidth, Sw.FrameHeight);

        public ISwMemoryGeometryBuilder MemoryGeometryBuilder { get; private set; }

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

        private IXLogger m_Logger;

        internal IServiceProvider Services { get; private set; }

        public bool IsAlive 
        {
            get
            {
                if (Process == null || Process.HasExited || !Process.Responding)
                {
                    return false;
                }
                else
                {
                    try
                    {
                        var testWnd = WindowHandle;
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        private bool m_IsInitialized;

        private bool m_HideOnStartup;
        
        private bool m_IsStartupNotified;

        private ElementCreator<ISldWorks> m_Creator;

        internal SwApplication(ISldWorks app, IXServiceCollection customServices) 
            : this(app)
        {
            Init(customServices);
        }

        /// <summary>
        /// Only to be used within SwAddInEx
        /// </summary>
        internal SwApplication(ISldWorks app)
        {
            m_IsStartupNotified = false;

            m_Creator = new ElementCreator<ISldWorks>(CreateInstance, app, true);
            WatchStartupCompleted((SldWorks)app);
        }

        /// <summary>
        /// Remarks used for <see cref="SwApplicationFactory.PreCreate"/>
        /// </summary>
        internal SwApplication()
        {
            m_IsStartupNotified = false;

            m_Creator = new ElementCreator<ISldWorks>(CreateInstance, null, false);

            m_Creator.CachedProperties.Set(new ServiceCollection(), nameof(CustomServices));
        }
        
        internal void Init(IXServiceCollection customServices)
        {
            if (!m_IsInitialized)
            {
                m_CustomServices = customServices;

                m_IsInitialized = true;

                var services = new ServiceCollection();

                ConfigureServices?.Invoke(this, services);
                OnConfigureServices(services);

                if (customServices != null)
                {
                    services.Merge(customServices);
                }

                Services = services.CreateProvider();
                m_Logger = Services.GetService<IXLogger>();

                m_DocumentsLazy = new Lazy<ISwDocumentCollection>(() => new SwDocumentCollection(this, m_Logger));

                var geomBuilderDocsProvider = Services.GetService<IMemoryGeometryBuilderDocumentProvider>();

                MemoryGeometryBuilder = new SwMemoryGeometryBuilder(this, geomBuilderDocsProvider);
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
                    return new SwVstaMacro(Sw, path);

                case VBA_FILE_EXT:
                case BASIC_EXT:
                    return new SwVbaMacro(Sw, path);

                default:
                    throw new NotSupportedException("Specified file is not a SOLIDWORKS macro");
            }
        }

        public void Dispose()
        {
            if (m_DocumentsLazy.IsValueCreated) 
            {
                m_DocumentsLazy.Value.Dispose();
            }

            if (Sw != null)
            {
                if (Marshal.IsComObject(Sw))
                {
                    Marshal.ReleaseComObject(Sw);
                }
            }
        }

        public void Close()
        {
            Sw.ExitApp();
        }
        
        public void Commit(CancellationToken cancellationToken)
        {
            m_Creator.Create(cancellationToken);
            Init(CustomServices);
        }

        private ISldWorks CreateInstance(CancellationToken cancellationToken)
        {
            m_HideOnStartup = State.HasFlag(ApplicationState_e.Hidden);

            using (var appStarter = new SwApplicationStarter(State, Version)) 
            {
                var app = appStarter.Start(cancellationToken);
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
            const int S_OK = 0;

            Debug.Assert(!m_IsStartupNotified, "This event shoud only be fired once");
            
            if (!m_IsStartupNotified)
            {
                m_IsStartupNotified = true;

                var continueListening = false;

                if (Sw?.StartupProcessCompleted == true)
                {
                    if (m_HideOnStartup)
                    {
                        const int HIDE = 0;
                        ShowWindow(new IntPtr(Sw.IFrameObject().GetHWnd()), HIDE);

                        Sw.Visible = false;
                    }

                    FirstStartupCompleted?.Invoke(this);
                }
                else
                {
                    continueListening = true;
                }

                if (!continueListening)
                {
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

            return S_OK;
        }

        private ApplicationState_e GetApplicationState() 
        {
            //TODO: find the state
            return ApplicationState_e.Default;
        }

        public void OnConfigureServices(IXServiceCollection collection)
        {
            collection.AddOrReplace((Func<IXLogger>)(() => new TraceLogger("xCAD.SwApplication")));
            collection.AddOrReplace((Func<IMemoryGeometryBuilderDocumentProvider>)(() => new DefaultMemoryGeometryBuilderDocumentProvider(this)));
            collection.AddOrReplace<IFilePathResolver>(() => new SwFilePathResolverNoSearchFolders(this));//TODO: there is some issue with recursive search of folders in search locations - do a test to validate
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
    }

    public static class SwApplicationExtension 
    {
        public static bool IsVersionNewerOrEqual(this ISwApplication app, SwVersion_e version, 
            int? servicePack = null, int? servicePackRev = null) 
        {
            return app.Sw.IsVersionNewerOrEqual(version, servicePack, servicePackRev);
        }
    }
}
//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Delegates;
using Xarial.XCad.Documents;
using Xarial.XCad.Enums;
using Xarial.XCad.Geometry;
using Xarial.XCad.Services;
using Xarial.XCad.SwDocumentManager.Documents;
using Xarial.XCad.SwDocumentManager.Services;
using Xarial.XCad.Toolkit;
using Xarial.XCad.Utils.Diagnostics;

namespace Xarial.XCad.SwDocumentManager
{
    /// <summary>
    /// SOLIDWORKS Document Manager Application
    /// </summary>
    public interface ISwDmApplication : IXApplication
    {
        /// <summary>
        /// Pointer to native Documetn Manager
        /// </summary>
        ISwDMApplication SwDocMgr { get; }

        /// <summary>
        /// License key
        /// </summary>
        SecureString LicenseKey { get; set; }

        /// <summary>
        /// Custom DI services
        /// </summary>
        IXServiceCollection CustomServices { get; set; }

        /// <summary>
        /// Documents collection
        /// </summary>
        new ISwDmDocumentCollection Documents { get; }

        /// <summary>
        /// Document manager version
        /// </summary>
        new ISwDmVersion Version { get; }
    }

    internal class SwDmApplication : ISwDmApplication, IXServiceConsumer
    {
        public event ConfigureServicesDelegate ConfigureServices;

        #region Not Supported        

        public event ApplicationStartingDelegate Starting { add => throw new NotSupportedException(); remove => throw new NotSupportedException(); }
        public event ApplicationIdleDelegate Idle { add => throw new NotSupportedException(); remove => throw new NotSupportedException(); }

        public ApplicationState_e State { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public bool IsAlive => throw new NotSupportedException();
        public Rectangle WindowRectangle => throw new NotSupportedException();
        public IntPtr WindowHandle => throw new NotSupportedException();
        public Process Process => throw new NotSupportedException();
        public IXMemoryGeometryBuilder MemoryGeometryBuilder => throw new NotSupportedException();
        public IXProgress CreateProgress() => throw new NotSupportedException();
        public IXMacro OpenMacro(string path) => throw new NotSupportedException();
        public MessageBoxResult_e ShowMessageBox(string msg, MessageBoxIcon_e icon = MessageBoxIcon_e.Info, MessageBoxButtons_e buttons = MessageBoxButtons_e.Ok) => throw new NotSupportedException();
        public void ShowTooltip(ITooltipSpec spec) => throw new NotSupportedException();
        public IXObjectTracker CreateObjectTracker(string name) => throw new NotSupportedException();
        public IXApplicationOptions Options => throw new NotSupportedException();
        public IXMaterialsDatabaseRepository MaterialDatabases => throw new NotSupportedException();
        #endregion

        IXDocumentRepository IXApplication.Documents => Documents;

        IXVersion IXApplication.Version
        {
            get => Version;
            set => throw new Exception("This property is read-only"); 
        }

        public ISwDmVersion Version => SwDmApplicationFactory.CreateVersion(VersionMapper.FromFileRevision(SwDocMgr.GetLatestSupportedFileVersion()));

        public ISwDmDocumentCollection Documents { get; }

        public bool IsCommitted => m_Creator.IsCreated;

        public ISwDMApplication SwDocMgr => m_Creator.Element;

        public SecureString LicenseKey 
        {
            get 
            {
                if (!IsCommitted)
                {
                    return m_Creator.CachedProperties.Get<SecureString>();
                }
                else 
                {
                    throw new NotSupportedException("This property is only available on creation of application");
                }
            }
            set 
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else 
                {
                    throw new NotSupportedException("");
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

        private bool m_IsDisposed;
        private bool m_IsClosed;

        private readonly IElementCreator<ISwDMApplication> m_Creator;

        internal IXLogger Logger { get; private set; }
        internal ISwVersionMapper VersionMapper { get; private set; }
        internal IFilePathResolver FilePathResolver { get; private set; }

        internal IServiceProvider Services { get; private set; }

        private IXServiceCollection m_CustomServices;

        private bool m_IsInitialized;

        internal SwDmApplication(ISwDMApplication dmApp, IXServiceCollection customServices) : this()
        {
            m_Creator = new ElementCreator<ISwDMApplication>(CreateApplication, dmApp, true);
            
            m_CustomServices = customServices;

            Init();
        }

        internal SwDmApplication() 
        {
            Documents = new SwDmDocumentCollection(this);

            m_Creator = new ElementCreator<ISwDMApplication>(CreateApplication, null, false);
        }

        internal void Init()
        {
            if (!m_IsInitialized)
            {
                m_IsInitialized = true;

                if (m_CustomServices == null)
                {
                    m_CustomServices = new ServiceCollection();
                }

                LoadServices(m_CustomServices);

                Services = m_CustomServices.CreateProvider();

                Logger = Services.GetService<IXLogger>();
                VersionMapper = Services.GetService<ISwVersionMapper>();
                FilePathResolver = Services.GetService<IFilePathResolver>();
            }
            else
            {
                Debug.Assert(false, "App has been already initialized. Must be only once");
            }
        }

        private void LoadServices(IXServiceCollection customServices)
        {
            customServices.Add<IXLogger>(() => new TraceLogger("xCAD.SwDmApplication"), ServiceLifetimeScope_e.Singleton, false);
            customServices.Add<IFilePathResolver>(() => new SwDmFilePathResolver(), ServiceLifetimeScope_e.Singleton, false);
            customServices.Add<ISwVersionMapper, SwDmVersionMapper>(ServiceLifetimeScope_e.Singleton, false);

            ConfigureServices?.Invoke(this, customServices);
        }

        private ISwDMApplication CreateApplication(CancellationToken cancellationToken)
        {
            var licKey = LicenseKey;
            LicenseKey = null;
            return SwDmApplicationFactory.ConnectToDm(licKey);
        }

        public void Close()
        {
            if (!m_IsClosed)
            {
                m_IsClosed = true;

                foreach (var doc in Documents.ToArray())
                {
                    if (doc.IsCommitted && doc.IsAlive)
                    {
                        doc.Close();
                    }
                }

                Dispose();
            }
        }

        public void Commit(CancellationToken cancellationToken)
        {
            m_Creator.Create(cancellationToken);
            Init();
        }

        public void Dispose()
        {
            if (!m_IsDisposed)
            {
                m_IsDisposed = true;

                if (!m_IsClosed)
                {
                    Close();
                }

                if (Marshal.IsComObject(SwDocMgr))
                {
                    Marshal.ReleaseComObject(SwDocMgr);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }

    public static class SwDmApplicationExtension
    {
        public static bool IsVersionNewerOrEqual(this ISwDmApplication app, SwDmVersion_e version) 
            => app.Version.IsVersionNewerOrEqual(version);
    }
}

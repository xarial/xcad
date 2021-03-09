//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Delegates;
using Xarial.XCad.Documents;
using Xarial.XCad.Enums;
using Xarial.XCad.Geometry;
using Xarial.XCad.Services;
using Xarial.XCad.SwDocumentManager.Documents;

namespace Xarial.XCad.SwDocumentManager
{
    public interface ISwDmApplication : IXApplication
    {
        ISwDMApplication SwDocMgr { get; }
        SecureString LicenseKey { get; set; }
        new ISwDmDocumentCollection Documents { get; }
        new ISwDmVersion Version { get; }
    }

    internal class SwDmApplication : ISwDmApplication
    {
        #region Not Supported        

        public event ApplicationStartingDelegate Starting;
        public ApplicationState_e State { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public bool IsAlive => throw new NotSupportedException();
        public Rectangle WindowRectangle => throw new NotSupportedException();
        public IntPtr WindowHandle => throw new NotSupportedException();
        public Process Process => throw new NotSupportedException();
        public IXMemoryGeometryBuilder MemoryGeometryBuilder => throw new NotSupportedException();
        public IXProgress CreateProgress() => throw new NotSupportedException();
        public IXMacro OpenMacro(string path) => throw new NotSupportedException();
        public void Close() => throw new NotSupportedException();
        public MessageBoxResult_e ShowMessageBox(string msg,
            MessageBoxIcon_e icon = MessageBoxIcon_e.Info, MessageBoxButtons_e buttons = MessageBoxButtons_e.Ok)
            => throw new NotSupportedException();

        #endregion

        IXDocumentRepository IXApplication.Documents => Documents;

        IXVersion IXApplication.Version
        {
            get => Version;
            set => throw new Exception("This property is read-only"); 
        }

        public ISwDmVersion Version
        {
            get => SwDmApplicationFactory.CreateVersion((SwDmVersion_e)SwDocMgr.GetLatestSupportedFileVersion());
        }

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

        private ElementCreator<ISwDMApplication> m_Creator;

        internal SwDmApplication(ISwDMApplication dmApp, bool isCreated) 
        {
            m_Creator = new ElementCreator<ISwDMApplication>(CreateApplication, dmApp, isCreated);
            Documents = new SwDmDocumentCollection(this);
        }

        private ISwDMApplication CreateApplication(CancellationToken cancellationToke)
            => SwDmApplicationFactory.ConnectToDm(LicenseKey);

        public void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);
    }

    public static class SwApplicationExtension
    {
        public static bool IsVersionNewerOrEqual(this ISwDmApplication app, SwDmVersion_e version)
            => app.Version.Major >= version;
    }
}

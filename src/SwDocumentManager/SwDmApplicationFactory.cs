//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.SwDocumentManager.Exceptions;

namespace Xarial.XCad.SwDocumentManager
{
    /// <summary>
    /// Provides a factory to create instance of the <see cref="ISwDmApplication"/>
    /// </summary>
    public static class SwDmApplicationFactory
    {
        public static ISwDmApplication PreCreate() => new SwDmApplication(null, false);

        public static ISwDmApplication Create(string dmKey) 
        {
            var app = PreCreate();
            app.LicenseKey = new NetworkCredential("", dmKey).SecurePassword;
            app.Commit();

            return app;
        }

        public static ISwDmApplication FromPointer(ISwDMApplication app) => new SwDmApplication(app, true);

        internal static ISwDMApplication ConnectToDm(SecureString dmKeySecure) 
        {
            ISwDMClassFactory classFactory = null;

            var classFactoryType = Type.GetTypeFromProgID("SwDocumentMgr.SwDMClassFactory");

            if (classFactoryType != null)
            {
                classFactory = Activator.CreateInstance(classFactoryType) as ISwDMClassFactory;
            }

            if (classFactory != null)
            {
                var dmKey = new NetworkCredential("", dmKeySecure).Password;

                try
                {
                    var dmApp = classFactory.GetApplication(dmKey);

                    if (dmApp != null)
                    {
                        var testVers = dmApp.GetLatestSupportedFileVersion();
                        return dmApp;
                    }
                    else 
                    {
                        throw new Exception("Application is null");
                    }
                }
                catch (Exception ex)
                {
                    throw new SwDmConnectFailedException(ex);
                }
            }
            else
            {
                throw new SwDmSdkNotInstalledException();
            }
        }

        public static ISwDmVersion CreateVersion(SwDmVersion_e major) => new SwDmVersion(new Version((int)major, 0));
    }
}

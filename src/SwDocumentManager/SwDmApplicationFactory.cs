﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Microsoft.Win32;
using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private const string DM_CLASS_FACT_PROG_ID = "SwDocumentMgr.SwDMClassFactory";

        /// <summary>
        /// Pre-creates application
        /// </summary>
        /// <returns>xCAD application</returns>
        public static ISwDmApplication PreCreate() => new SwDmApplication(null, false);

        /// <summary>
        /// Returns all installed SOLIDWORKS Document Manager versions
        /// </summary>
        /// <returns>Enumerates versions</returns>
        /// <remarks>Latest supported file version of the application also depends on the version of the Document Manager license key</remarks>
        public static IEnumerable<ISwDmVersion> GetInstalledVersions()
        {
            var swDmAppRegKey = Registry.ClassesRoot.OpenSubKey($"{DM_CLASS_FACT_PROG_ID}\\CLSID");

            if (swDmAppRegKey != null)
            {
                var clsid = (string)swDmAppRegKey.GetValue("");

                var swDmAppClsidRegKey = Registry.ClassesRoot.OpenSubKey($"CLSID\\{clsid}\\InprocServer32");

                if (swDmAppClsidRegKey != null) 
                {
                    var dmDllPath = (string)swDmAppClsidRegKey.GetValue("");

                    if (File.Exists(dmDllPath))
                    {
                        var majorVers = FileVersionInfo.GetVersionInfo(dmDllPath).FileMajorPart;

                        //only support SW 2000 or newer
                        if (majorVers >= 8) 
                        {
                            var dmVersList = ((SwDmVersion_e[])Enum.GetValues(typeof(SwDmVersion_e))).ToList();

                            var dmVersInd = dmVersList.IndexOf(SwDmVersion_e.Sw2000) + (majorVers - 8);

                            if (dmVersInd < dmVersList.Count)
                            {
                                var dmVers = dmVersList[dmVersInd];

                                yield return CreateVersion(dmVers);
                            }
                            else 
                            {
                                throw new NotSupportedException($"File versions {majorVers} cannot be converted to Document Manager version");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates an instance of the application from the Document Manager key
        /// </summary>
        /// <param name="dmKey">Document manager key</param>
        /// <returns>xCAD application</returns>
        public static ISwDmApplication Create(string dmKey) 
        {
            var app = PreCreate();
            app.LicenseKey = new NetworkCredential("", dmKey).SecurePassword;
            app.Commit();

            return app;
        }

        /// <summary>
        /// Creates instance of the application from the COM pointer
        /// </summary>
        /// <param name="app">Pointer to the application</param>
        /// <returns>xCAD application</returns>
        public static ISwDmApplication FromPointer(ISwDMApplication app) => new SwDmApplication(app, true);

        internal static ISwDMApplication ConnectToDm(SecureString dmKeySecure) 
        {
            ISwDMClassFactory classFactory = null;

            var classFactoryType = Type.GetTypeFromProgID(DM_CLASS_FACT_PROG_ID);

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
                        throw new NullReferenceException("Application is null");
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

        /// <summary>
        /// Creates a version of the application
        /// </summary>
        /// <param name="major">Major version</param>
        /// <returns>Version</returns>
        public static ISwDmVersion CreateVersion(SwDmVersion_e major) => new SwDmVersion(new Version((int)major, 0));
    }
}

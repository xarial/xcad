//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.IO;
using System.Linq;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.Toolkit.Windows;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Win32;
using Xarial.XCad.Toolkit;
using Xarial.XCad.Enums;
using Xarial.XCad.Utils.Diagnostics;
using Xarial.XCad.SolidWorks.Services;

namespace Xarial.XCad.SolidWorks
{
    /// <summary>
    /// Factory for creating <see cref="ISwApplication"/>
    /// </summary>
    public class SwApplicationFactory
    {
        internal class SwVersionInfo
        {
            internal string ProgId { get; }
            internal string ExePath { get; }
            internal int Revision { get; }
            internal SwVersion_e Version { get; }

            internal SwVersionInfo(string progId, string exePath, SwVersion_e vers, int rev)
            {
                ProgId = progId;
                ExePath = exePath;
                Version = vers;
                Revision = rev;
            }
        }

        internal static class CommandLineArguments
        {
            /// <summary>
            /// Bypasses the Tools/Options settings
            /// </summary>
            public const string SafeMode = "/SWSafeMode /SWDisableExitApp";

            /// <summary>
            /// Runs SOLIDWORKS in background model via SOLIDWORKS Task Scheduler (requires SOLIDWORKS Professional or higher)
            /// </summary>
            public const string BackgroundMode = "/b";

            /// <summary>
            /// Suppresses all popup messages, including the splash screen
            /// </summary>
            public const string SilentMode = "/r";
        }

        internal const string PROG_ID_BASE_NAME = "SldWorks.Application.";
        
        private const string ADDINS_STARTUP_REG_KEY = @"Software\SolidWorks\AddInsStartup";

        private static readonly SwVersionMapper m_VersionMapper;

        static SwApplicationFactory() 
        {
            m_VersionMapper = new SwVersionMapper();
        }

        /// <summary>
        /// Disables all startup add-ins
        /// </summary>
        /// <param name="disabledAddInGuids">Guids of the disabled add-ins</param>
        /// <remarks>Call the <see cref="EnableAddInsStartup(IReadOnlyList{string})"/> to restore the add-ins</remarks>
        public static void DisableAllAddInsStartup(out IReadOnlyList<string> disabledAddInGuids)
        {
            const int DISABLE_VAL = 0;
            const int ENABLE_VAL = 1;

            var localDisabledAddInGuids = new List<string>();

            var addinsStartup = Registry.CurrentUser.OpenSubKey(ADDINS_STARTUP_REG_KEY, true);

            if (addinsStartup != null)
            {
                var addInKeyNames = addinsStartup.GetSubKeyNames();

                if (addInKeyNames != null)
                {
                    foreach (var addInKeyName in addInKeyNames)
                    {
                        var addInKey = addinsStartup.OpenSubKey(addInKeyName, true);

                        int enableVal;

                        if (int.TryParse(addInKey.GetValue("")?.ToString(), out enableVal))
                        {
                            var loadOnStartup = enableVal == ENABLE_VAL;

                            if (loadOnStartup)
                            {
                                addInKey.SetValue("", DISABLE_VAL);
                                localDisabledAddInGuids.Add(addInKeyName);
                            }
                        }
                    }
                }
            }

            disabledAddInGuids = localDisabledAddInGuids;
        }

        /// <summary>
        /// Enables the add-ins at startup
        /// </summary>
        /// <param name="addInGuids">Add-in guids</param>
        public static void EnableAddInsStartup(IReadOnlyList<string> addInGuids)
        {
            const int ENABLE_VAL = 1;

            var addinsStartup = Registry.CurrentUser.OpenSubKey(ADDINS_STARTUP_REG_KEY, true);

            foreach (var addInKeyName in addInGuids)
            {
                var addInKey = addinsStartup.OpenSubKey(addInKeyName, true);

                addInKey.SetValue("", ENABLE_VAL);
            }
        }

        /// <summary>
        /// Pre-creates a template for SOLIDWORKS application
        /// </summary>
        /// <returns></returns>
        public static ISwApplication PreCreate() => new SwApplication();

        /// <summary>
        /// Returns all installed SOLIDWORKS versions
        /// </summary>
        /// <returns>Enumerates versions</returns>
        public static IEnumerable<ISwVersion> GetInstalledVersions() 
            => GetInstalledVersionInfos().Select(v => CreateVersion(v.Version));

        internal static IEnumerable<SwVersionInfo> GetInstalledVersionInfos()
        {
            foreach (var swProgIdKey in Registry.ClassesRoot.GetSubKeyNames().Where(k => k.StartsWith(PROG_ID_BASE_NAME, StringComparison.CurrentCultureIgnoreCase)))
            {
                var swAppRegKey = Registry.ClassesRoot.OpenSubKey(swProgIdKey, false);

                if (swAppRegKey != null)
                {
                    var isInstalled = false;

                    int appRev;
                    string path;

                    try
                    {
                        appRev = int.Parse(swProgIdKey.Substring(PROG_ID_BASE_NAME.Length));

                        path = FindSwPathFromRegKey(swAppRegKey);

                        if (!File.Exists(path)) 
                        {
                            throw new Exception("Installation path is not found");
                        }

                        isInstalled = true;
                    }
                    catch
                    {
                        appRev = -1;
                        path = "";
                    }

                    if (isInstalled)
                    {
                        var vers = m_VersionMapper.FromApplicationRevision(appRev);

                        yield return new SwVersionInfo(swProgIdKey, path, vers, appRev);
                    }
                }
            }
        }

        /// <summary>
        /// Creates <see cref="ISwApplication"/> from SOLIDWORKS pointer
        /// </summary>
        /// <param name="app">Pointer to SOLIDWORKS application</param>
        /// <returns>Instance of <see cref="ISwApplication"/></returns>
        public static ISwApplication FromPointer(ISldWorks app)
            => FromPointer(app, new ServiceCollection());

        /// <inheritdoc cref="FromPointer(ISldWorks)"/>
        /// <param name="services">Custom services</param>
        public static ISwApplication FromPointer(ISldWorks app, IXServiceCollection services)
            => new SwApplication(app, services);

        /// <summary>
        /// Creates instance of SOLIDWORKS from SLDWORKS.exe process
        /// </summary>
        /// <param name="process">SLDWORKS.exe process</param>
        /// <returns>Pointer to <see cref="ISwApplication"/></returns>
        public static ISwApplication FromProcess(Process process)
            => FromProcess(process, new ServiceCollection());

        /// <inheritdoc cref="FromProcess(Process)"/>
        /// <param name="services">Custom services</param>
        public static ISwApplication FromProcess(Process process, IXServiceCollection services)
        {
            var app = RotHelper.TryGetComObjectByMonikerName<ISldWorks>(GetMonikerName(process), new TraceLogger("xCAD.SwApplication"));

            if (app != null)
            {
                return FromPointer(app, services);
            }
            else
            {
                throw new Exception($"Cannot access SOLIDWORKS application at process {process.Id}");
            }
        }

        /// <summary>
        /// Starts new application
        /// </summary>
        /// <param name="vers">Version or null for the latest</param>
        /// <param name="state">State of the application</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created application</returns>
        public static ISwApplication Create(SwVersion_e? vers = null,
            ApplicationState_e state = ApplicationState_e.Default,
            CancellationToken cancellationToken = default)
        {
            var app = PreCreate();

            app.Version = vers.HasValue ? CreateVersion(vers.Value) : null;
            app.State = state;

            app.Commit(cancellationToken);

            return app;
        }

        /// <summary>
        /// Creates instance of SOLIDWORKS version from the major version
        /// </summary>
        /// <param name="vers">Version</param>
        /// <returns>Version instance</returns>
        public static ISwVersion CreateVersion(SwVersion_e vers) => new SwVersion(new Version((int)vers, 0, 0), vers, 0, 0, m_VersionMapper.GetVersionName(vers));

        /// <summary>
        /// Creates instance of SOLIDWORKS version from the release year
        /// </summary>
        /// <param name="releaseYear">Release year</param>
        /// <returns>Version instance</returns>
        public static ISwVersion CreateVersionFromReleaseYear(int releaseYear) => CreateVersion(m_VersionMapper.FromReleaseYear(releaseYear));

        /// <summary>
        /// Creates instance of SOLIDOWRKS version from the revision number
        /// </summary>
        /// <param name="revision">Revision number</param>
        /// <returns>Version instance</returns>
        public static ISwVersion CreateVersionFromRevision(int revision) => CreateVersion(m_VersionMapper.FromApplicationRevision(revision));

        internal static string GetMonikerName(Process process) => $"SolidWorks_PID_{process.Id}";

        internal static string FindSwPathFromRegKey(RegistryKey swAppRegKey)
        {
            var clsidKey = swAppRegKey.OpenSubKey("CLSID", false);

            if (clsidKey == null)
            {
                throw new NullReferenceException($"Incorrect registry value, CLSID is missing");
            }

            var clsid = (string)clsidKey.GetValue("");

            if (clsid == null)
            {
                throw new NullReferenceException($"Incorrect registry value, LocalServer32 is missing");
            }

            var localServerKey = Registry.ClassesRoot.OpenSubKey(
                $"CLSID\\{clsid}\\LocalServer32", false);

            if (localServerKey == null) 
            {
                throw new Exception("Failed to find the class id in the registry. Make sure that application is running as x64 bit process (including 'Prefer 32-bit' option is unchecked in the project settings)");
            }

            var swAppPath = (string)localServerKey.GetValue("");

            if (!File.Exists(swAppPath))
            {
                throw new FileNotFoundException($"Path to SOLIDWORKS executable does not exist: {swAppPath}");
            }

            return swAppPath;
        }
    }
}
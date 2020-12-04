//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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
using Xarial.XCad.SolidWorks.Exceptions;
using System.Collections.Generic;
using Microsoft.Win32;
using Xarial.XCad.Toolkit;
using Xarial.XCad.Enums;

namespace Xarial.XCad.SolidWorks
{
    public class SwApplicationFactory
    {
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

        internal const string PROG_ID_TEMPLATE = "SldWorks.Application.{0}";
        
        private const string ADDINS_STARTUP_REG_KEY = @"Software\SolidWorks\AddInsStartup";

        public static void DisableAllAddInsStartup(out List<string> disabledAddInGuids)
        {
            const int DISABLE_VAL = 0;
            const int ENABLE_VAL = 1;

            disabledAddInGuids = new List<string>();

            var addinsStartup = Registry.CurrentUser.OpenSubKey(ADDINS_STARTUP_REG_KEY, true);

            if (addinsStartup != null)
            {
                var addInKeyNames = addinsStartup.GetSubKeyNames();

                if (addInKeyNames != null)
                {
                    foreach (var addInKeyName in addInKeyNames)
                    {
                        var addInKey = addinsStartup.OpenSubKey(addInKeyName, true);

                        var loadOnStartup = (int)addInKey.GetValue("") == ENABLE_VAL;

                        if (loadOnStartup)
                        {
                            addInKey.SetValue("", DISABLE_VAL);
                            disabledAddInGuids.Add(addInKeyName);
                        }
                    }
                }
            }
        }

        public static void EnableAddInsStartup(List<string> addInGuids)
        {
            const int ENABLE_VAL = 1;

            var addinsStartup = Registry.CurrentUser.OpenSubKey(ADDINS_STARTUP_REG_KEY, true);

            foreach (var addInKeyName in addInGuids)
            {
                var addInKey = addinsStartup.OpenSubKey(addInKeyName, true);

                addInKey.SetValue("", ENABLE_VAL);
            }
        }

        public static ISwApplication PreCreate() => new SwApplication();

        public static IEnumerable<ISwVersion> GetInstalledVersions()
        {
            foreach (var versCand in Enum.GetValues(typeof(SwVersion_e)).Cast<SwVersion_e>())
            {
                var progId = string.Format(PROG_ID_TEMPLATE, (int)versCand);
                var swAppRegKey = Registry.ClassesRoot.OpenSubKey(progId);

                if (swAppRegKey != null)
                {
                    var isInstalled = false;

                    try
                    {
                        FindSwPathFromRegKey(swAppRegKey);
                        isInstalled = true;
                    }
                    catch
                    {
                    }

                    if (isInstalled)
                    {
                        yield return CreateVersion(versCand);
                    }
                }
            }
        }

        public static ISwApplication FromPointer(ISldWorks app)
            => FromPointer(app, new ServiceCollection());

        public static ISwApplication FromPointer(ISldWorks app, IXServiceCollection services)
        {
            return new SwApplication(app, services);
        }

        public static ISwApplication FromProcess(Process process)
            => FromProcess(process, new ServiceCollection());

        public static ISwApplication FromProcess(Process process, IXServiceCollection services)
        {
            var app = RotHelper.TryGetComObjectByMonikerName<ISldWorks>(GetMonikerName(process));

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
            CancellationToken? cancellationToken = null)
        {
            var app = PreCreate();

            app.Version = vers.HasValue ? CreateVersion(vers.Value) : null;
            app.State = state;

            var token = CancellationToken.None;

            if (cancellationToken.HasValue) 
            {
                token = cancellationToken.Value;
            }

            app.Commit(token);

            return app;
        }

        public static ISwVersion CreateVersion(SwVersion_e vers) => new SwVersion(vers);

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
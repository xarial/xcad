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

namespace Xarial.XCad.SolidWorks
{
    public class SwApplicationFactory
    {
        public static class CommandLineArguments
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

        private const string PROG_ID_TEMPLATE = "SldWorks.Application.{0}";

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

        private static string GetMonikerName(Process process) => $"SolidWorks_PID_{process.Id}";

        ///<inheritdoc cref="Start(SwVersion_e?, string, CancellationToken?)"/>
        ///<remarks>Default timeout is 5 minutes. Use different overload of this method to specify custom cancellation token</remarks>
        public static ISwApplication Start(SwVersion_e? vers = null,
            string args = "") => Start(vers, args, new CancellationTokenSource(TimeSpan.FromMinutes(5)).Token);

        /// <summary>
        /// Starts new instance of the SOLIDWORKS application
        /// </summary>
        /// <param name="vers">Version of SOLIDWORKS to start or null for the latest version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Instance of application</returns>
        public static ISwApplication Start(SwVersion_e? vers,
            string args, CancellationToken? cancellationToken = null)
            => Start(vers, args, new ServiceCollection(), cancellationToken);

        ///<inheritdoc cref="Start(SwVersion_e?, string, CancellationToken?)"/>
        /// <param name="logger">Logger</param>
        public static ISwApplication Start(SwVersion_e? vers,
            string args, IXServiceCollection services, CancellationToken? cancellationToken = null)
        {
            var swPath = FindSwAppPath(vers);

            var prc = Process.Start(swPath, args);

            try
            {
                ISldWorks app = null;

                do
                {
                    if (cancellationToken.HasValue)
                    {
                        if (cancellationToken.Value.IsCancellationRequested)
                        {
                            throw new AppStartCancelledByUserException();
                        }
                    }

                    app = RotHelper.TryGetComObjectByMonikerName<ISldWorks>(GetMonikerName(prc));
                    Thread.Sleep(100);
                }
                while (app == null);

                return FromPointer(app, services);
            }
            catch
            {
                if (prc != null)
                {
                    try
                    {
                        prc.Kill();
                    }
                    catch
                    {
                    }
                }

                throw;
            }
        }

        public static IEnumerable<SwVersion_e> GetInstalledVersions()
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
                        yield return versCand;
                    }
                }
            }
        }

        private static string FindSwAppPath(SwVersion_e? vers)
        {
            RegistryKey swAppRegKey = null;

            if (vers.HasValue)
            {
                var progId = string.Format(PROG_ID_TEMPLATE, (int)vers);
                swAppRegKey = Registry.ClassesRoot.OpenSubKey(progId);
            }
            else
            {
                foreach (var versCand in Enum.GetValues(typeof(SwVersion_e)).Cast<int>().OrderByDescending(x => x))
                {
                    var progId = string.Format(PROG_ID_TEMPLATE, versCand);
                    swAppRegKey = Registry.ClassesRoot.OpenSubKey(progId);

                    if (swAppRegKey != null)
                    {
                        break;
                    }
                }
            }

            if (swAppRegKey != null)
            {
                return FindSwPathFromRegKey(swAppRegKey);
            }
            else
            {
                throw new NullReferenceException("Failed to find the information about the installed SOLIDWORKS applications in the registry");
            }
        }

        private static string FindSwPathFromRegKey(RegistryKey swAppRegKey)
        {
            var clsidKey = swAppRegKey.OpenSubKey("CLSID", false);

            if (clsidKey == null)
            {
                throw new NullReferenceException($"Incorrect registry value, CLSID is missing");
            }

            var clsid = (string)clsidKey.GetValue("");

            var localServerKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(
                $"CLSID\\{clsid}\\LocalServer32", false);

            if (clsid == null)
            {
                throw new NullReferenceException($"Incorrect registry value, LocalServer32 is missing");
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
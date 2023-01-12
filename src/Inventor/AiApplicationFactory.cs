//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Toolkit.Windows;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using Xarial.XCad.Toolkit;
using Xarial.XCad.Enums;
using Inventor;
using Xarial.XCad.Inventor.Enums;
using Xarial.XCad.Utils.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using Xarial.XCad.Inventor.Utils;
using Xarial.XCad.Base;

namespace Xarial.XCad.Inventor
{
    /// <summary>
    /// Factory for creating <see cref="IAiApplication"/>
    /// </summary>
    public class AiApplicationFactory
    {
        internal const string PROG_ID_TEMPLATE = "Inventor.Application.{0}";
        internal const string APP_MONIKER_NAME = "!{B6B5DC40-96E3-11D2-B774-0060B0F159EF}";

        private const string REG_PATH = @"SOFTWARE\Autodesk\Inventor";
        private const string REG_KEY_TEMPLATE = "RegistryVersion{0}.{1}";

        internal static RegistryKey OpenRegistryVersionKey(AiVersion_e specVerc)
        {
            string regVersKeyName;

            if (specVerc == AiVersion_e.Inventor5dot3)
            {
                regVersKeyName = string.Format(REG_KEY_TEMPLATE, 5, 3);
            }
            else
            {
                regVersKeyName = string.Format(REG_KEY_TEMPLATE, (int)specVerc, 0);
            }

            return Registry.LocalMachine.OpenSubKey(REG_PATH + @"\" + regVersKeyName, false);
        }

        internal static string GetApplicationPathFromRegistryVersionKey(RegistryKey regVersKey)
        {
            var path = (string)regVersKey.GetValue("InventorLocation");

            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("Inventor location path is empty");
            }

            var exeLoc = System.IO.Path.Combine(path, "Inventor.exe");

            if (System.IO.File.Exists(exeLoc))
            {
                return exeLoc;
            }
            else
            {
                throw new FileNotFoundException($"Inventor executable file '{exeLoc}' is not found");
            }
        }

        /// <summary>
        /// Pre-creates a template for Inventor application
        /// </summary>
        /// <returns></returns>
        public static IAiApplication PreCreate() => new AiApplication();

        /// <summary>
        /// Returns all installed Inventor versions
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IAiVersion> GetInstalledVersions()
        {
            foreach (var versCand in Enum.GetValues(typeof(AiVersion_e)).Cast<AiVersion_e>())
            {
                var regKey = OpenRegistryVersionKey(versCand);

                if (regKey != null)
                {
                    var isInstalled = false;

                    try
                    {
                        GetApplicationPathFromRegistryVersionKey(regKey);
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

        /// <summary>
        /// Creates <see cref="IAiApplication"/> from Inventor pointer
        /// </summary>
        /// <param name="app">Pointer to Inventor application</param>
        /// <returns>Instance of <see cref="IAiApplication"/></returns>
        public static IAiApplication FromPointer(Application app)
            => FromPointer(app, new ServiceCollection());

        /// <inheritdoc cref="FromPointer(Application)"/>
        /// <param name="services">Custom serives</param>
        public static IAiApplication FromPointer(Application app, IXServiceCollection services)
            => new AiApplication(app, services);

        /// <summary>
        /// Creates instance of Inventor from Inventor.exe process
        /// </summary>
        /// <param name="process">Inventor.exe process</param>
        /// <returns>Pointer to <see cref="IAiApplication"/></returns>
        public static IAiApplication FromProcess(Process process)
            => FromProcess(process, new ServiceCollection());

        /// <inheritdoc cref="FromProcess(Process)"/>
        /// <param name="services">Custom serives</param>
        public static IAiApplication FromProcess(Process process, IXServiceCollection services)
        {
            if (TryGetApplicationFromProcess(process, new TraceLogger("xCAD.AiApplication"), out var invApp))
            {
                return FromPointer(invApp, services);
            }
            else
            {
                throw new Exception($"Cannot access Inventor application at process {process.Id}");
            }
        }

        internal static bool TryGetApplicationFromProcess(Process process, IXLogger logger, out Application app)
        {
            bool ValidateApplicationPointer(Application appPtr)
            {
                WinAPI.GetWindowThreadProcessId(new IntPtr(appPtr.MainFrameHWND), out var prcId);
                return prcId == process.Id;
            }

            app = RotHelper.TryGetComObjectByMonikerName<Application>(APP_MONIKER_NAME, logger,
                ValidateApplicationPointer);

            if (app == null)
            {
                var doc = RotHelper.TryGetComObjectByMonikerName<Document>("", new TraceLogger("xCAD.AiApplication"),
                    d => ValidateApplicationPointer((Application)d.Parent));

                if (doc != null)
                {
                    app = (Application)doc.Parent;
                }
            }

            if (app == null)
            {
                const string MONIKER_NAME_TEMPLATE = "!XCad_Inventor_Appication_{0}";
                
                app = RotHelper.TryGetComObjectByMonikerName<Application>(string.Format(MONIKER_NAME_TEMPLATE, process.Id), logger,
                    ValidateApplicationPointer);
            }

            return app != null;
        }

        /// <summary>
        /// Starts new application
        /// </summary>
        /// <param name="vers">Version or null for the latest</param>
        /// <param name="state">State of the application</param>
        /// <param name="strategy">Strategy for running new application</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created application</returns>
        public static IAiApplication Create(AiVersion_e? vers = null,
            ApplicationState_e state = ApplicationState_e.Default,
            StartApplicationConnectStrategy_e strategy = StartApplicationConnectStrategy_e.Default,
            CancellationToken cancellationToken = default)
        {
            var app = PreCreate();

            app.StartApplicationConnectStrategy = strategy;
            app.Version = vers.HasValue ? CreateVersion(vers.Value) : null;
            app.State = state;

            app.Commit(cancellationToken);

            return app;
        }

        /// <summary>
        /// Creates instance of Inventor version from the major version
        /// </summary>
        /// <param name="vers">Major version</param>
        /// <returns>Version</returns>
        public static IAiVersion CreateVersion(AiVersion_e vers) => new AiVersion(new Version((int)vers, 0));
    }
}
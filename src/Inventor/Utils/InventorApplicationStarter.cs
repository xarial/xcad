//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Inventor;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Enums;
using Xarial.XCad.Inventor;
using Xarial.XCad.Inventor.Enums;
using Xarial.XCad.Inventor.Properties;
using Xarial.XCad.Toolkit.Windows;

namespace Xarial.XCad.Inventor.Utils
{
    internal class InventorApplicationStarter : IDisposable
    {
        private class LazyTempTokenFile : IDisposable
        {
            internal string FilePath => m_FilePathLazy.Value;

            private readonly IXLogger m_Logger;

            private readonly Lazy<string> m_FilePathLazy;

            private Application m_App;

            internal LazyTempTokenFile(IXLogger logger) 
            {
                m_Logger = logger;

                m_FilePathLazy = new Lazy<string>(() => 
                {
                    var filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "_XCadAppToken_" + Guid.NewGuid().ToString() + ".DWG");
                    System.IO.File.WriteAllBytes(filePath, Resources.XCadAppToken);
                    return filePath;
                });
            }

            internal void SetApplication(Application app) 
            {
                m_App = app;
            }

            public void Dispose()
            {
                if (m_FilePathLazy.IsValueCreated)
                {
                    if (m_App != null) 
                    {
                        for (int i = 0; i < m_App.Documents.Count; i++) 
                        {
                            var doc = m_App.Documents[i];
                            
                            if (string.Equals(doc.FullFileName, m_FilePathLazy.Value, StringComparison.CurrentCultureIgnoreCase)) 
                            {
                                doc.Close();
                            }
                        }
                    }

                    try
                    {
                        m_Logger.Log($"Deleting temp file: '{FilePath}'");
                        System.IO.File.Delete(FilePath);
                    }
                    catch (Exception ex)
                    {
                        m_Logger.Log(ex);
                    }
                }
            }
        }

        private readonly ApplicationState_e m_State;
        private readonly IAiVersion m_Version;

        private readonly IXLogger m_Logger;

        private readonly StartApplicationConnectStrategy_e Strategy;

        internal InventorApplicationStarter(ApplicationState_e state, IAiVersion version, StartApplicationConnectStrategy_e strategy, IXLogger logger) 
        {
            m_State = state;
            m_Version = version;

            Strategy = strategy;

            m_Logger = logger;
        }

        internal Application Start(Action<Process> startHandler, CancellationToken cancellationToken) 
        {
            AiVersion_e? vers = null;

            if (m_Version != null)
            {
                vers = m_Version.Major;
            }

            var args = new List<string>();

            if (m_State.HasFlag(ApplicationState_e.Safe))
            {
                //TODO: handle safe mode
            }

            if (m_State.HasFlag(ApplicationState_e.Background))
            {
                //TODO: handle background mode
            }

            var inventorAppPath = FindInventorAppPath(vers);

            using (var tempTokenFile = new LazyTempTokenFile(m_Logger))
            {
                var isFirstInstance = !Process.GetProcessesByName("Inventor").Any();

                if (!isFirstInstance && Strategy.HasFlag(StartApplicationConnectStrategy_e.AllowCreatingTempTokenDocuments))
                {
                    args.Add($"\"{tempTokenFile.FilePath}\"");
                }

                var startArgs = string.Join(" ", args);

                var prcInfo = new ProcessStartInfo(inventorAppPath, startArgs);

                if (m_State.HasFlag(ApplicationState_e.Hidden))
                {
                    prcInfo.UseShellExecute = false;
                    prcInfo.CreateNoWindow = true;
                    prcInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }

                m_Logger.Log($"Starting Inventor application from '{inventorAppPath}' with arguments: '{startArgs}'", LoggerMessageSeverity_e.Debug);

                Process prc = null;

                try
                {
                    prc = Process.Start(prcInfo);

                    startHandler.Invoke(prc);

                    Application app = null;

                    do
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException();
                        }

                        if (prc.HasExited)
                        {
                            m_Logger.Log("Inventor process has exited before moniker was found", LoggerMessageSeverity_e.Debug);

                            throw new Exception($"Inventor process has exited");
                        }

                        if (app == null)
                        {
                            if (AiApplicationFactory.TryGetApplicationFromProcess(prc, m_Logger, out app))
                            {
                                tempTokenFile.SetApplication(app);
                            }
                        }

                        Thread.Sleep(100);
                    }
                    while (app == null || (Strategy.HasFlag(StartApplicationConnectStrategy_e.WaitUntilFullyLoaded) && !app.Ready));

                    if (m_State.HasFlag(ApplicationState_e.Hidden))
                    {
                        m_Logger.Log($"Hiding Inventor application {prc.Id}", LoggerMessageSeverity_e.Debug);
                        app.Visible = false;
                    }

                    if (m_State.HasFlag(ApplicationState_e.Silent))
                    {
                        app.SilentOperation = true;
                    }

                    return app;
                }
                catch (Exception ex)
                {
                    m_Logger.Log(ex);

                    if (prc != null)
                    {
                        try
                        {
                            prc.Kill();
                        }
                        catch (Exception prcEx)
                        {
                            m_Logger.Log(prcEx);
                        }
                    }

                    throw;
                }
            }
        }

        private string FindInventorAppPath(AiVersion_e? vers)
        {
            if (vers.HasValue)
            {
                var rev = Convert.ToInt32(vers.Value);

                foreach (var instVers in AiApplicationFactory.GetInstalledVersionInfos()) 
                {
                    if (instVers.Major == rev) 
                    {
                        if (vers == AiVersion_e.Inventor5dot3)
                        {
                            if (instVers.Minor != 3) 
                            {
                                continue;
                            }
                        }

                        return instVers.ExePath;
                    }
                }

                throw new NullReferenceException("Failed to find the specified version");
            }
            else
            {
                var newestVersion = AiApplicationFactory.GetInstalledVersionInfos().OrderBy(v => v.Major).LastOrDefault();

                if (newestVersion != null)
                {
                    return newestVersion.ExePath;
                }
                else
                {
                    throw new Exception("No installed Inventor versions found");
                }
            }
        }

        public void Dispose()
        {
        }
    }
}

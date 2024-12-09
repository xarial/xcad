//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Microsoft.Win32;
using SolidWorks.Interop.sldworks;
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
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SolidWorks.Exceptions;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Toolkit.Windows;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal class SwApplicationStarter : IDisposable
    {
        private readonly ApplicationState_e m_State;
        private readonly ISwVersion m_Version;

        internal SwApplicationStarter(ApplicationState_e state, ISwVersion version) 
        {
            m_State = state;
            m_Version = version;
        }

        internal ISldWorks Start(Action<Process> startHandler, IXLogger logger, CancellationToken cancellationToken) 
        {
            SwVersion_e? vers = null;

            if (m_Version != null)
            {
                vers = m_Version.Major;
            }

            var args = new List<string>();

            if (m_State.HasFlag(ApplicationState_e.Safe))
            {
                args.Add(SwApplicationFactory.CommandLineArguments.SafeMode);
            }

            if (m_State.HasFlag(ApplicationState_e.Silent))
            {
                args.Add(SwApplicationFactory.CommandLineArguments.SilentMode);
            }

            if (m_State.HasFlag(ApplicationState_e.Background))
            {
                args.Add(SwApplicationFactory.CommandLineArguments.BackgroundMode);
            }

            var swPath = FindSwAppPath(vers);

            var startArgs = string.Join(" ", args);

            var prcInfo = new ProcessStartInfo(swPath, startArgs);

            if (m_State.HasFlag(ApplicationState_e.Hidden))
            {
                prcInfo.UseShellExecute = false;
                prcInfo.CreateNoWindow = true;
                prcInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }

            logger.Log($"Starting SOLIDWORKS application from '{swPath}' with arguments: '{startArgs}'", LoggerMessageSeverity_e.Debug);

            var prc = Process.Start(prcInfo);

            startHandler.Invoke(prc);

            try
            {
                ISldWorks app;
                do
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new AppStartCancelledByUserException();
                    }

                    if (prc.HasExited) 
                    {
                        logger.Log("SOLIDWORKS process has exited before moniker was found", LoggerMessageSeverity_e.Debug);

                        throw new Exception($"SOLIDWORKS process has exited");
                    }

                    app = RotHelper.TryGetComObjectByMonikerName<ISldWorks>(SwApplicationFactory.GetMonikerName(prc), logger);
                    Thread.Sleep(100);
                }
                while (app == null);

                if (m_State.HasFlag(ApplicationState_e.Hidden)) 
                {
                    logger.Log($"Hiding SOLIDWORKS application {prc.Id}", LoggerMessageSeverity_e.Debug);
                    app.Visible = false;
                }

                return app;
            }
            catch(Exception ex)
            {
                logger.Log(ex);

                if (prc != null)
                {
                    try
                    {
                        prc.Kill();
                    }
                    catch(Exception prcEx)
                    {
                        logger.Log(prcEx);
                    }
                }

                throw;
            }
        }

        private string FindSwAppPath(SwVersion_e? vers)
        {
            RegistryKey swAppRegKey = null;

            if (vers.HasValue)
            {
                var progId = SwApplicationFactory.PROG_ID_BASE_NAME + Convert.ToInt32(vers.Value);
                swAppRegKey = Registry.ClassesRoot.OpenSubKey(progId, false);

                if (swAppRegKey != null)
                {
                    return SwApplicationFactory.FindSwPathFromRegKey(swAppRegKey);
                }
                else
                {
                    throw new NullReferenceException("Failed to find the information about the installed SOLIDWORKS applications in the registry");
                }
            }
            else
            {
                var newestVersion = SwApplicationFactory.GetInstalledVersionInfos().OrderBy(v => v.Revision).LastOrDefault();

                if (newestVersion != null)
                {
                    return newestVersion.ExePath;
                }
                else 
                {
                    throw new Exception("No installed SOLIDWORKS versions found");
                }
            }
        }

        public void Dispose()
        {
            
        }
    }
}

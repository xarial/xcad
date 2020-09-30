//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Windows;
using Xarial.XCad.Utils.Diagnostics;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using Xarial.XCad.SolidWorks.Exceptions;
using Xarial.XCad.Base;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Xarial.XCad.SolidWorks
{
    /// <inheritdoc/>
    public class SwApplication : IXApplication, IDisposable
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
            public const string SuppressPopups = "/r";
        }

        private const string PROG_ID_TEMPLATE = "SldWorks.Application.{0}";

        public static SwApplication FromPointer(ISldWorks app)
        {
            return new SwApplication(app, new TraceLogger("xCAD"));
        }

        public static SwApplication FromProcess(Process process)
        {
            var app = RotHelper.TryGetComObjectByMonikerName<ISldWorks>(GetMonikerName(process));

            if (app != null)
            {
                return FromPointer(app);
            }
            else
            {
                throw new Exception($"Cannot access SOLIDWORKS application at process {process.Id}");
            }
        }

        private static string GetMonikerName(Process process) => $"SolidWorks_PID_{process.Id}";

        ///<inheritdoc cref="Start(SwVersion_e?, string, CancellationToken?)"/>
        ///<remarks>Default timeout is 5 minutes. Use different overload of this method to specify custom cancellation token</remarks>
        public static SwApplication Start(SwVersion_e? vers = null,
            string args = "")
        {
            return Start(vers, args, new CancellationTokenSource(TimeSpan.FromMinutes(5)).Token);
        }

        /// <summary>
        /// Starts new instance of the SOLIDWORKS application
        /// </summary>
        /// <param name="vers">Version of SOLIDWORKS to start or null for the latest version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Instance of application</returns>
        public static SwApplication Start(SwVersion_e? vers,
            string args, CancellationToken? cancellationToken = null)
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

                return FromPointer(app);
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

        IXDocumentCollection IXApplication.Documents => Documents;
        IXGeometryBuilder IXApplication.GeometryBuilder => GeometryBuilder;
        IXMacro IXApplication.OpenMacro(string path) => OpenMacro(path);

        public ISldWorks Sw { get; private set; }

        public SwVersion_e Version => Sw.GetVersion();
        
        public SwDocumentCollection Documents { get; private set; }

        public SwGeometryBuilder GeometryBuilder { get; private set; }

        public IntPtr WindowHandle => new IntPtr(Sw.IFrameObject().GetHWndx64());

        public Process Process => Process.GetProcessById(Sw.GetProcessID());

        internal SwApplication(ISldWorks app, IXLogger logger)
        {
            Sw = app;
            Documents = new SwDocumentCollection(this, logger);
            GeometryBuilder = new SwGeometryBuilder(app.IGetMathUtility(), app.IGetModeler());
        }

        public MessageBoxResult_e ShowMessageBox(string msg, MessageBoxIcon_e icon = MessageBoxIcon_e.Info, MessageBoxButtons_e buttons = MessageBoxButtons_e.Ok)
        {
            swMessageBoxBtn_e swBtn = 0;
            swMessageBoxIcon_e swIcon = 0;

            switch (icon)
            {
                case MessageBoxIcon_e.Info:
                    swIcon = swMessageBoxIcon_e.swMbInformation;
                    break;

                case MessageBoxIcon_e.Question:
                    swIcon = swMessageBoxIcon_e.swMbQuestion;
                    break;

                case MessageBoxIcon_e.Error:
                    swIcon = swMessageBoxIcon_e.swMbStop;
                    break;

                case MessageBoxIcon_e.Warning:
                    swIcon = swMessageBoxIcon_e.swMbWarning;
                    break;
            }

            switch (buttons)
            {
                case MessageBoxButtons_e.Ok:
                    swBtn = swMessageBoxBtn_e.swMbOk;
                    break;

                case MessageBoxButtons_e.YesNo:
                    swBtn = swMessageBoxBtn_e.swMbYesNo;
                    break;

                case MessageBoxButtons_e.OkCancel:
                    swBtn = swMessageBoxBtn_e.swMbOkCancel;
                    break;

                case MessageBoxButtons_e.YesNoCancel:
                    swBtn = swMessageBoxBtn_e.swMbYesNoCancel;
                    break;
            }

            var swRes = (swMessageBoxResult_e)Sw.SendMsgToUser2(msg, (int)swIcon, (int)swBtn);

            switch (swRes)
            {
                case swMessageBoxResult_e.swMbHitOk:
                    return MessageBoxResult_e.Ok;

                case swMessageBoxResult_e.swMbHitCancel:
                    return MessageBoxResult_e.Cancel;

                case swMessageBoxResult_e.swMbHitYes:
                    return MessageBoxResult_e.Yes;

                case swMessageBoxResult_e.swMbHitNo:
                    return MessageBoxResult_e.No;

                default:
                    return 0;
            }
        }

        public SwMacro OpenMacro(string path)
        {
            const string VSTA_FILE_EXT = ".dll";
            const string VBA_FILE_EXT = ".swp";
            const string BASIC_EXT = ".swb";

            var ext = Path.GetExtension(path);

            switch (ext.ToLower()) 
            {
                case VSTA_FILE_EXT:
                    return new SwVstaMacro(Sw, path);

                case VBA_FILE_EXT:
                case BASIC_EXT:
                    return new SwVbaMacro(Sw, path);

                default:
                    throw new NotSupportedException("Specified file is not a SOLIDWORKS macro");
            }
        }

        public void Dispose()
        {
            if (Sw != null)
            {
                if (Marshal.IsComObject(Sw))
                {
                    Marshal.ReleaseComObject(Sw);
                }
            }

            Sw = null;
        }

        public void Close()
        {
            Sw.ExitApp();
        }
    }

    public static class SwApplicationExtension 
    {
        public static bool IsVersionNewerOrEqual(this SwApplication app, SwVersion_e version, 
            int? servicePack = null, int? servicePackRev = null) 
        {
            return app.Sw.IsVersionNewerOrEqual(version, servicePack, servicePackRev);
        }
    }
}
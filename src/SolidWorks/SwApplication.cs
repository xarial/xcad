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
using System.Drawing;
using Xarial.XCad.Delegates;
using Xarial.XCad.Toolkit;
using Xarial.XCad.SolidWorks.Services;

namespace Xarial.XCad.SolidWorks
{
    /// <inheritdoc/>
    public partial class SwApplication : IXApplication, IXServiceConsumer, IDisposable
    {
        public event ApplicationLoadedDelegate Loaded;
        internal event Action<Type> PropertyPageOpening;
        internal event Action<Type> PropertyPageClosed;

        IXDocumentRepository IXApplication.Documents => Documents;

        IXMacro IXApplication.OpenMacro(string path) => OpenMacro(path);

        IXGeometryBuilder IXApplication.MemoryGeometryBuilder => MemoryGeometryBuilder;
        
        public ISldWorks Sw { get; private set; }

        public SwVersion_e Version => Sw.GetVersion();
        
        public SwDocumentCollection Documents { get; private set; }
        
        public IntPtr WindowHandle => new IntPtr(Sw.IFrameObject().GetHWndx64());

        public Process Process => Process.GetProcessById(Sw.GetProcessID());

        public Rectangle WindowRectangle => new Rectangle(Sw.FrameLeft, Sw.FrameTop, Sw.FrameWidth, Sw.FrameHeight);

        public SwMemoryGeometryBuilder MemoryGeometryBuilder { get; private set; }
        
        private IXLogger m_Logger;

        private IServiceProvider m_Provider;

        internal SwApplication(ISldWorks app, IXServiceCollection customServices)
        {
            Sw = app;
            Init(customServices);
        }

        /// <summary>
        /// Only to be used within SwAddInEx
        /// </summary>
        internal SwApplication(ISldWorks app)
        {
            Sw = app;
        }

        internal void Init(IXServiceCollection customServices) 
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            if (customServices != null)
            {
                services.Merge(customServices);
            }

            m_Provider = services.CreateProvider();
            m_Logger = m_Provider.GetService<IXLogger>();

            Documents = new SwDocumentCollection(this, m_Logger);

            var geomBuilderDocsProvider = m_Provider.GetService<IMemoryGeometryBuilderDocumentProvider>();

            MemoryGeometryBuilder = new SwMemoryGeometryBuilder(this, geomBuilderDocsProvider);

            (Sw as SldWorks).OnIdleNotify += OnLoadFirstIdleNotify;
        }

        internal void ReportPropertyPageOpening(Type pmpDataType) 
        {
            PropertyPageOpening?.Invoke(pmpDataType);
        }

        internal void ReportPropertyPageClosed(Type pmpDataType)
        {
            PropertyPageClosed?.Invoke(pmpDataType);
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

        private int OnLoadFirstIdleNotify()
        {
            const int S_OK = 0;

            var continueListening = false;

            if (Loaded != null)
            {
                if (Sw.StartupProcessCompleted)
                {
                    Loaded?.Invoke(this);
                }
                else 
                {
                    continueListening = true;
                }
            }

            if (!continueListening) 
            {
                (Sw as SldWorks).OnIdleNotify -= OnLoadFirstIdleNotify;
            }

            return S_OK;
        }

        public void ConfigureServices(IXServiceCollection collection)
        {
            collection.AddOrReplace<IXLogger>(() => new TraceLogger("xCAD.SwApplication"));
            collection.AddOrReplace<IMemoryGeometryBuilderDocumentProvider>(() => new DefaultMemoryGeometryBuilderDocumentProvider(this));
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
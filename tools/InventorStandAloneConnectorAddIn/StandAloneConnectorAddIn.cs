//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Inventor;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Xarial.XCad.Inventor.Tools
{
    [Guid("C1F5C04A-EA27-4ED0-A341-A32296CF9D3F")]
    public class StandAloneConnectorAddIn : ApplicationAddInServer
    {
        private const string TRACE_LOG_NAME = "xCAD.Inventor";

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        [DllImport("ole32.dll")]
        private static extern int CreateItemMoniker([MarshalAs(UnmanagedType.LPWStr)] string lpszDelim,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszItem, out IMoniker ppmk);

        private const string MONIKER_NAME_TEMPLATE = "!XCad_Inventor_Appication_{0}";

        private int? m_ComObjectId;

        private int m_ProcessId;

        public void Activate(ApplicationAddInSite AddInSiteObject, bool FirstTime)
        {
            var app = AddInSiteObject.Application;

            m_ProcessId = Process.GetCurrentProcess().Id;

            try
            {
                Trace.WriteLine($"Registering Inventor COM object in process {m_ProcessId}", TRACE_LOG_NAME);
                
                m_ComObjectId = RegisterComObject(app, string.Format(MONIKER_NAME_TEMPLATE, m_ProcessId), true, false);
                
                Trace.WriteLine($"Registered Inventor COM object in the process {m_ProcessId} as {m_ComObjectId}", TRACE_LOG_NAME);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to register Inventor COM object in process {m_ProcessId}: {ex.Message}", TRACE_LOG_NAME);
            }
        }

        public void Deactivate()
        {
            try
            {
                if (m_ComObjectId.HasValue)
                {
                    Trace.WriteLine($"Unregistering Inventor COM object in process {m_ProcessId}", TRACE_LOG_NAME);

                    UnregisterComObject(m_ComObjectId.Value);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to unregister Inventor COM object in process {m_ProcessId}: {ex.Message}", TRACE_LOG_NAME);
            }
        }

        public void ExecuteCommand(int CommandID)
        {
        }

        public object Automation => null;

        private int RegisterComObject(object obj, string monikerName, bool keepAlive = true, bool allowAnyClient = false)
        {
            IMoniker moniker = null;

            IBindCtx context;
            CreateBindCtx(0, out context);

            IRunningObjectTable rot;
            context.GetRunningObjectTable(out rot);

            try
            {
                const int ROTFLAGS_REGISTRATIONKEEPSALIVE = 1;
                const int ROTFLAGS_ALLOWANYCLIENT = 2;

                context.GetRunningObjectTable(out rot);

                const int S_OK = 0;

                if (CreateItemMoniker("", monikerName, out moniker) != S_OK)
                {
                    throw new Exception("Failed to create moniker");
                }

                var opts = 0;

                if (keepAlive)
                {
                    opts += ROTFLAGS_REGISTRATIONKEEPSALIVE;
                }

                if (allowAnyClient)
                {
                    opts += ROTFLAGS_ALLOWANYCLIENT;
                }

                var id = rot.Register(opts, obj, moniker);

                if (id == 0)
                {
                    throw new Exception("Failed to register object in ROT");
                }

                return id;
            }
            finally
            {
                if (moniker != null)
                {
                    while (Marshal.ReleaseComObject(moniker) > 0);
                }
                if (rot != null)
                {
                    while (Marshal.ReleaseComObject(rot) > 0);
                }
                if (context != null)
                {
                    while (Marshal.ReleaseComObject(context) > 0);
                }
            }
        }

        private void UnregisterComObject(int id)
        {
            IBindCtx context;
            CreateBindCtx(0, out context);

            IRunningObjectTable rot;
            context.GetRunningObjectTable(out rot);

            try
            {
                rot.Revoke(id);
            }
            finally
            {
                if (rot != null)
                {
                    while (Marshal.ReleaseComObject(rot) > 0);
                }

                if (context != null)
                {
                    while (Marshal.ReleaseComObject(context) > 0);
                }
            }
        }
    }
}

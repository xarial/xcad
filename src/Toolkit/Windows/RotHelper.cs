//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Xarial.XCad.Base;

namespace Xarial.XCad.Toolkit.Windows
{
    public static class RotHelper
    {
        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        [DllImport("ole32.dll")]
        private static extern int CreateItemMoniker([MarshalAs(UnmanagedType.LPWStr)] string lpszDelim,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszItem, out IMoniker ppmk);

        public static TComObj TryGetComObjectByMonikerName<TComObj>(string monikerName, IXLogger logger = null)
        {
            IBindCtx context = null;
            IRunningObjectTable rot = null;
            IEnumMoniker monikers = null;

            try
            {
                CreateBindCtx(0, out context);

                context.GetRunningObjectTable(out rot);
                rot.EnumRunning(out monikers);

                var moniker = new IMoniker[1];

                while (monikers.Next(1, moniker, IntPtr.Zero) == 0)
                {
                    var curMoniker = moniker.First();

                    string name = null;

                    if (curMoniker != null)
                    {
                        try
                        {
                            curMoniker.GetDisplayName(context, null, out name);
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            logger?.Log(ex);
                        }
                    }

                    if (string.Equals(monikerName,
                        name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        object app;
                        rot.GetObject(curMoniker, out app);
                        return (TComObj)app;
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.Log(ex);
            }
            finally
            {
                if (monikers != null)
                {
                    while (Marshal.ReleaseComObject(monikers) > 0) ;
                }

                if (rot != null)
                {
                    while (Marshal.ReleaseComObject(rot) > 0) ;
                }

                if (context != null)
                {
                    while (Marshal.ReleaseComObject(context) > 0) ;
                }
            }

            return default;
        }

        public static int RegisterComObject(object obj, string monikerName, bool keepAlive = true, bool allowAnyClient = false, IXLogger logger = null)
        {
            IBindCtx context = null;
            IRunningObjectTable rot = null;
            IMoniker moniker = null;

            CreateBindCtx(0, out context);
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

                logger?.Log($"Registering object in ROT with {opts} option", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);

                var id = rot.Register(opts, obj, moniker);

                if (id == 0)
                {
                    throw new Exception("Failed to register object in ROT");
                }

                logger?.Log($"Object id in ROT: {id}", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);

                return id;
            }
            catch (Exception ex)
            {
                logger?.Log(ex);
                throw;
            }
            finally
            {
                if (moniker != null)
                {
                    while (Marshal.ReleaseComObject(moniker) > 0) ;
                }
                if (rot != null)
                {
                    while (Marshal.ReleaseComObject(rot) > 0) ;
                }
                if (context != null)
                {
                    while (Marshal.ReleaseComObject(context) > 0) ;
                }
            }
        }

        public static void UnregisterComObject(int id, IXLogger logger = null)
        {
            IBindCtx context = null;
            IRunningObjectTable rot = null;

            CreateBindCtx(0, out context);
            context.GetRunningObjectTable(out rot);

            try
            {
                rot.Revoke(id);
            }
            catch (Exception ex)
            {
                logger?.Log(ex);
            }
            finally
            {
                if (rot != null)
                {
                    while (Marshal.ReleaseComObject(rot) > 0) ;
                }

                if (context != null)
                {
                    while (Marshal.ReleaseComObject(context) > 0) ;
                }
            }
        }
    }
}

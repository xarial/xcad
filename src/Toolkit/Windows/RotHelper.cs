//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
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
    /// <summary>
    /// Utilities for accessing Running Object Table
    /// </summary>
    public static class RotHelper
    {
        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        /// <summary>
        /// Returns the COM object by moniker name
        /// </summary>
        /// <typeparam name="TComObj">Type of COM object</typeparam>
        /// <param name="monikerName">Name of the moniker or an empty string to iterate all monikers</param>
        /// <param name="logger">Custom logger</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>COM objects</returns>
        public static TComObj TryGetComObjectByMonikerName<TComObj>(string monikerName, IXLogger logger, Predicate<TComObj> predicate = null)
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

                    if (curMoniker != null)
                    {
                        try
                        {
                            curMoniker.GetDisplayName(context, null, out var name);

                            if (string.IsNullOrEmpty(monikerName) || string.Equals(monikerName,
                                name, StringComparison.CurrentCultureIgnoreCase))
                            {
                                object app;
                                rot.GetObject(curMoniker, out app);

                                if (app is TComObj)
                                {
                                    if (predicate == null || predicate.Invoke((TComObj)app))
                                    {
                                        return (TComObj)app;
                                    }
                                }
                            }
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            logger?.Log(ex);
                        }
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
    }
}

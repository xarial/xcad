//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.SolidWorks.Enums;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal static class CompatibilityUtils
    {
        internal enum HighResIconsScope_e
        {
            CommandManager,
            TaskPane,
            MacroFeature
        }

        internal static bool SupportsHighResIcons(this ISldWorks app, HighResIconsScope_e scope)
        {
            switch (scope)
            {
                case HighResIconsScope_e.CommandManager:
                    return app.IsVersionNewerOrEqual(SwVersion_e.Sw2016);

                case HighResIconsScope_e.TaskPane:
                    return app.IsVersionNewerOrEqual(SwVersion_e.Sw2017);

                case HighResIconsScope_e.MacroFeature:
                    return app.IsVersionNewerOrEqual(SwVersion_e.Sw2017);

                default:
                    //Debug.Assert(false, "Not supported scope");
                    return false;
            }
        }

        /// <summary>
        /// Returns the major version of SOLIDWORKS application
        /// </summary>
        /// <param name="app">Pointer to application to return version from</param>
        /// <param name="servicePack">Version of Service Pack</param>
        /// <param name="servicePackRev">Revision of Service Pack</param>
        /// <returns>Major version of the application</returns>
        internal static SwVersion_e GetVersion(this ISldWorks app, out int servicePack, out int servicePackRev)
        {
            var rev = app.RevisionNumber().Split('.');
            var majorRev = int.Parse(rev[0]);
            servicePack = int.Parse(rev[1]);
            servicePackRev = int.Parse(rev[2]);

            return (SwVersion_e)majorRev;
        }

        /// <inheritdoc cref="GetVersion(ISldWorks, out int, out int)"/>
        internal static SwVersion_e GetVersion(this ISldWorks app)
        {
            int sp;
            int spRev;
            return app.GetVersion(out sp, out spRev);
        }

        /// <summary>
        /// Checks if the version of the SOLIDWORKS is newer or equal to the specified parameters
        /// </summary>
        /// <param name="app">Current SOLIDWORKS application</param>
        /// <param name="version">Target minimum supported version of SOLIDWORKS</param>
        /// <param name="servicePack">Target minimum service pack version or null to ignore</param>
        /// <param name="servicePackRev">Target minimum revision of service pack version or null to ignore</param>
        /// <returns>True of version of the SOLIDWORKS is the same or newer</returns>
        internal static bool IsVersionNewerOrEqual(this ISldWorks app, SwVersion_e version, int? servicePack = null, int? servicePackRev = null)
        {
            if (!servicePack.HasValue && servicePackRev.HasValue)
            {
                throw new ArgumentException($"{nameof(servicePack)} must be specified when {nameof(servicePackRev)} is specified");
            }

            int curSp;
            int curSpRev;
            var curVers = GetVersion(app, out curSp, out curSpRev);

            if (curVers >= version)
            {
                if (servicePack.HasValue && curVers == version)
                {
                    if (curSp >= servicePack.Value)
                    {
                        if (servicePackRev.HasValue)
                        {
                            return curSpRev >= servicePackRev.Value;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }
    }
}
using System;
using System.Runtime.InteropServices;
using Xarial.XCad.Base;
using Xarial.XCad.SolidWorks;

namespace Xarial.XCad.Documentation
{
    [ComVisible(true), Guid("CD7222A3-A70E-421A-8998-46FC184AEDDC")]
    public class LogAddIn : SwAddInEx
    {
        internal const string LOGGER_NAME = "MyAddInLog";

        public override void OnConnect()
        {
            try
            {
                Logger.Log("Loading add-in...");

                //implement connection
            }
            catch (Exception ex)
            {
                Logger.Log(ex, true, Base.Enums.LoggerMessageSeverity_e.Fatal);
                throw;
            }
        }
    }
}

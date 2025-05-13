﻿using SolidWorks.Interop.sldworks;
using System;
using System.Runtime.InteropServices;
using Xarial.XCad.SolidWorks;

namespace Xarial.XCad.Documentation
{
    [ComVisible(true), Guid("CA8A02D2-802F-46E3-A16A-DE69FADFF5B2")]
    public class MyAddIn : SwAddInEx
    {
        #region Connect
        public override void OnConnect()
        {
            // Initialize the add-in, create menu, load data etc.
        }
        #endregion Connect

        #region Disconnect
        public override void OnDisconnect()
        {
            // Dispose the add-in's resources
        }
        #endregion Disconnect

        #region SwObjects
        private void AccessSwObjects()
        {
            int addInCookie = this.AddInId;
            ISldWorks swApp = this.Application.Sw;
            ICommandManager cmdMgr = this.CommandManager.CmdMgr;
        }
        #endregion SwObjects
    }
}

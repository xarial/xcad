using System;
using System.Runtime.InteropServices;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.Commands.Structures;

namespace Xarial.XCad.Documentation
{
    [Title("Custom enable state add-in")]
    [ComVisible(true), Guid("E02CA015-09D7-4428-A2D8-C2E20E0EA52E")]
    public class CustomEnableAddIn : SwAddInEx
    {
        #region CustomEnableState
        public enum Commands_e
        {
            Command1,
            Command2
        }

        public override void OnConnect()
        {
            this.CommandManager.AddCommandGroup<Commands_e>().CommandStateResolve += OnButtonEnable;
        }

        private void OnButtonEnable(Commands_e cmd, CommandState state)
        {
            switch (cmd)
            {
                case Commands_e.Command1:
                case Commands_e.Command2:
                    //implement logic to identify the state of the button
                    state.Checked = false;
                    state.Enabled = false;
                    break;
            }
        }
        #endregion CustomEnableState
    }
}

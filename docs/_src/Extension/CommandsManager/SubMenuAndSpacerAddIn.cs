using System;
using System.Runtime.InteropServices;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands;

[Title("Custom enable state add-in")]
[ComVisible(true), Guid("37854E10-C3F8-41AA-AB46-614EC712DF42")]
public class SubMenuAndSpacerAddIn : SwAddInEx
{
    //--- SpacerAndSubMenu
    [Title("AddInEx Commands")]
    //--- Spacer
    public enum Commands_e
    {
        Command1,

        [CommandSpacer]
        Command2
    }
    //---

    [Title("Sub Menu Commands")]
    //--- SubMenu
    [CommandGroupParent(typeof(Commands_e))]
    public enum SubCommands_e
    {
        SubCommand1,
        SubCommand2
    }
    //---

    public override void OnConnect()
    {
        this.CommandManager.AddCommandGroup<Commands_e>().CommandClick += OnButtonClick;
        this.CommandManager.AddCommandGroup<SubCommands_e>().CommandClick += OnButtonClick;
    }

    private void OnButtonClick(Commands_e cmd)
    {
    }

    private void OnButtonClick(SubCommands_e cmd)
    {
    }
    //---
}


using SolidWorks.Interop.swconst;
using System.Runtime.InteropServices;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.UI.Commands;

[Title("Commands add-in")]
[ComVisible(true), Guid("9F9AB0BB-549B-4885-BDA2-05AEA702E550")]
public class CommandsAddIn : SwAddInEx
{
    #region HostingCommands
    public override void OnConnect()
    {
        #region CommandGroup
        this.CommandManager.AddCommandGroup<CommandsA_e>().CommandClick += OnCommandsAButtonClick;
        this.CommandManager.AddCommandGroup<CommandsB_e>().CommandClick += OnCommandsBButtonClick;
        this.CommandManager.AddCommandGroup<CommandsC_e>().CommandClick += OnCommandsCButtonClick;
        #endregion CommandGroup
        #region ContextMenu
        this.CommandManager.AddContextMenu<CommandsD_e>().CommandClick += OnCommandsDContextMenuClick;
        this.CommandManager.AddContextMenu<CommandsE_e>().CommandClick += OnCommandsEContextMenuClick;
        #endregion ContextMenu
    }

    #region CommandGroup2
    private void OnCommandsAButtonClick(CommandsA_e cmd)
    {
        //handle the button click
    }

    private void OnCommandsBButtonClick(CommandsB_e cmd)
    {
        //handle the button click
    }

    private void OnCommandsCButtonClick(CommandsC_e cmd)
    {
        //handle the button click
    }
    #endregion CommandGroup2
    #region ContextMenu2
    private void OnCommandsDContextMenuClick(CommandsD_e cmd)
    {
        //handle the context menu click
    }

    private void OnCommandsEContextMenuClick(CommandsE_e cmd)
    {
        //handle the context menu click
    }
    #endregion ContextMenu2
    #endregion HostingCommands
}
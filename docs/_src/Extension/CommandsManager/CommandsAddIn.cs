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
    //--- HostingCommands
    public override void OnConnect()
    {
        //--- CommandGroup
        this.CommandManager.AddCommandGroup<CommandsA_e>().CommandClick += OnCommandsAButtonClick;
        this.CommandManager.AddCommandGroup<CommandsB_e>().CommandClick += OnCommandsBButtonClick;
        this.CommandManager.AddCommandGroup<CommandsC_e>().CommandClick += OnCommandsCButtonClick;
        //---
        //--- ContextMenu
        this.CommandManager.AddContextMenu<CommandsD_e>().CommandClick += OnCommandsDContextMenuClick;
        this.CommandManager.AddContextMenu<CommandsE_e>(SelectType_e.Faces).CommandClick+= OnCommandsEContextMenuClick;
        //---
    }

    //--- CommandGroup
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
    //---
    //--- ContextMenu
    private void OnCommandsDContextMenuClick(CommandsD_e cmd)
    {
        //handle the context menu click
    }

    private void OnCommandsEContextMenuClick(CommandsE_e cmd)
    {
        //handle the context menu click
    }
    //---
    //---
}
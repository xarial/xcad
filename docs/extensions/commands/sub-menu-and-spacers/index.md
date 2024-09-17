---
title: Adding sub-menus and spacers to SOLIDWORKS command manager using xCAD
caption: Sub-Menus And Spacers
description: Adding sub-menus and spacers or command tab boxes in SOLIDWORKS command manager using xCAD framework
image: sub-menu-and-spacer.png
order: 3
---
## Adding spacer

Spacer can be added between the commands by decorating the command using the **CommandSpacerAttribute**. Spacer will be added before this command.

<<< @/_src/Extension\CommandsManager\SubMenuAndSpacerAddIn.cs#Spacer

If command tab tab boxes are created for this command group (i.e. *showInCmdTabBox* parameter is set to *true* in the **CommandItemInfoAttribute**), spacer is not reflected in the corresponding command tab box.

## Adding sub-menus

Sub-menus for the command groups can be defined by calling the corresponding overload of the **CommandGroupParent** attribute and specifying the type of the parent menu group or the user id

<<< @/_src/Extension\CommandsManager\SubMenuAndSpacerAddIn.cs#SubMenu

Sub menus are rendered in separate tab boxes in the command tab.

## Example

<<< @/_src/Extension\CommandsManager\SubMenuAndSpacerAddIn.cs#SpacerAndSubMenu

The above commands configuration would result in the following menu and command tab boxes created:

![Sub-menus and spacer](sub-menu-and-spacer.png)

* Command1 and Command2 are commands of the top level menu defined in Commands_e enumeration
* Spacer is added between Command1 and Command2
* SubCommand1 and SubCommand2 are commands of SubCommands_e enumeration which is a sub menu of Commands_e enumeration

![Command tab boxes](command-tab.png)

* All commands (including sub menu commands) are added on the same command tab
* Command1 and Command2 are placed in a separate command tab boxes of SubCommand1 and SubCommand2
* Spacer between Command1 and Command2 is ignored in the commands tab

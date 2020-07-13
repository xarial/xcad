---
title: Hosting SOLIDWORKS commands in menu, toolbar and context menu using xCAD
caption: Hosting Commands
description: Hosting options for SOLIDWORKS commands using xCAD (command group, context menu, toolbar and commands tab box)
image: commands-toolbar.png
order: 2
---
[Defined commands](/extension/commands/defining-commands/) can be hosted in different locations of SOLIDWORKS commands area: [command group](#command-group), which includes [menu](#menu), [toolbar](#toolbar) and [command tab box (ribbon)](#command-tab-box) as well as in the [context menu](#context-menu)

## Command Group

In order to add command group it is required to call the **AddCommandGroup** method and pass the enumeration type as a generic parameter.

It is required to provide the void handler function with a parameter of enumerator which will be called by framework when command is clicked.

{% code-snippet { file-name: ~Extension\CommandsManager\CommandsAddIn.*, regions: [CommandGroup] } %}

### Menu

![Commands displayed in the SOLIDWORKS menu](commands-menu.png){ width=350 }

By default command will be added to menu and [toolbar](#toolbar). This behavior can be changed by assigning the *hasMenu* boolean parameter of the **CommandItemInfoAttribute** attribute.

### Toolbar

![Commands displayed in the SOLIDWORKS toolbar](commands-toolbar.png){ width=350 }

By default command will be added to [menu](#menu) and toolbar. This behavior can be changed by assigning the *hasToolbar* boolean parameter of the **CommandItemInfoAttribute** attribute.

### Command Tab Box

![Commands added to command tab box](command-tab.png){ width=450 }

Command item can be added to tab box by setting the *showInCmdTabBox* parameter of 
**CommandItemInfoAttribute** to *true* for the specific command defined in the enumeration.

*textStyle* parameter allows to specify the alignment of the hint text relative to the icon.

![Text display styles in command tab box](command-tab-box-text-display.png){ width=250 }

* Icon only (without text) (NoText)
* Text below icon (TextBelow)
* Text to the right to icon, aligned horizontally (TextHorizontal)

{% code-snippet { file-name: ~Extension\CommandsManager\CommandTabBox.* } %}

## Context Menu

![Commands displayed in the context menu](commands-context-menu.png){ width=250 }

In order to add context menu it is required to call the **AddContextMenu** method and pass the enumeration as a template parameter.

It is required to provide the void handler function with a parameter of enumeration which will be called by framework when command is clicked.

It is optionally required to specify the selection type of where this menu should be displayed.

{% code-snippet { file-name: ~Extension\CommandsManager\CommandsAddIn.*, regions: [ContextMenu] } %}



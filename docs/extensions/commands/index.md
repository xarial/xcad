---
title: Adding commands into the SOLIDWORKS menu and toolbar using xCAD
caption: Commands
description: Registering and handling SOLIDWORKS commands in menu, toolbar and context menu using xCAD. Customizing the look of commands by providing custom icons, titles and tooltips.
order: 4
---
Commands can be defined by creating an enumerations. Commands can be customized by adding attributes to assign title, tooltip, icon etc. Commands can be grouped under sub menus. Simply specify the image (transparency is supported) and framework will create required bitmaps compatible with SOLIDWORKS. No need to assign gray background to enable transparency, no need to scale images to fit the required sizes - simply use any image and framework will do the rest. Use resources to localize the add-in.

~~~ cs  jagged
using Xarial.XCad.UI.Commands;
~~~

~~~ cs jagged
[Title(typeof(Resources), nameof(Resources.ToolbarTitle)), Description("Toolbar with commands")]
[Icon(typeof(Resources), nameof(Resources.commands))]
public enum Commands_e
{
    [Title("Command 1"), Description("Sample command 1")]
    [Icon(typeof(Resources), nameof(Resources.command1))]
    [CommandItemInfo(true, true, WorkspaceTypes_e.Assembly, true, RibbonTabTextDisplay_e.TextBelow)]
    Command1,
    Command2
}
~~~

~~~ cs jagged
CommandManager.AddCommandGroup<Commands_e>().CommandClick += OnCommandClick;
~~~

~~~ cs jagged
private void OnCommandClick(Commands_e spec)
{
    //TODO: handle commands
}
~~~
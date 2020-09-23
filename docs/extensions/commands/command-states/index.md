---
title: Custom enable command state for SOLIDWORKS commands
caption: Custom Enable Command State
description: Explanation on using the custom enable states for the SOLIDWORKS commands using xCAD framework
image: command-states.png
order: 4
---
There are 4 command states supported by SOLIDWORKS:

1. Deselected and enabled. This is default option when button can be clicked
1. Deselected and disabled. This option is used when the command is not supported in certain framework. For example mate command will be disabled in parts and drawings as it is only supported in the assemblies.
1. Selected and disabled. This represents the disabled checked button
1. Selected and enabled. This represents checked button

![Supported command states](command-states.png)

SwEx framework will assign the appropriate state (enabled or disabled) for the commands based on their supported workspaces if defined in the **CommandItemInfoAttribute**. However user can alter the state to provide more advanced management (for example it might be required to enable command if certain object is selected or if any bodies or components are present in the model). To do this it is required to specify to subscribe to **IXCommandGroup::CommandStateResolve** event. **IXCommandGroup** is created as the result of calling **AddCommandGroup** or **AddContextMenu** methods.

The value of state will be preassigned based on the workspace and can be changed by the user within the method.

> This method allows to implement the toggle button in toolbar and menu. To set the checked state use the *Checked*.

{% code-snippet { file-name: ~Extension\CommandsManager\CustomEnableAddIn.*, regions: [CustomEnableState] } %}

Refer [Toggle Command Example](https://github.com/xarial/xcad-examples/tree/master/ToggleCommand) for the demonstration of how to achieve check box effect for toolbar button in SOLIDWORKS using the command states.
---
title: Adding commands into the SOLIDWORKS menu and toolbar using xCAD
caption: Commands
description: Registering and handling SOLIDWORKS commands in menu, toolbar and context menu using xCAD. Customizing the look of commands by providing custom icons, titles and tooltips.
order: 4
---
Commands can be defined by creating an enumerations.

Commands can be customized by adding attributes to assign title, tooltip, icon etc.

Commands can be grouped under sub menus. Simply specify the image (transparency is supported) and framework will create required bitmaps compatible with SOLIDWORKS. No need to assign gray background to enable transparency, no need to scale images to fit the required sizes - simply use any image and framework will do the rest.

User can handle the commands clicks and assign the custom state for command buttons.

Multiple command groups can be inserted within the same add-in.

Use resources to localize the add-in.
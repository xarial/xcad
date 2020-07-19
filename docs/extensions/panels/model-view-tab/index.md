---
title: Adding custom Windows Forms and WPF control into SOLIDWORKS Model View Manager using xCAD
caption: Model View Tab
description: Instructions of how to add custom WPF and Windows Forms controls into the SOLIDWORKS Model View Manager using xCAD framework
image: model-view-manager.png
---
![Custom Model View Manager Tab](model-view-manager.png){ width=600 }

xCAD framework allows to add custom [Windows Forms controls](https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.usercontrol) and [WPF controls](https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.usercontrol) into Model View Manager tabs.

This functionality is only available for Part and Assembly documents

Decorate the control classes with **TitleAttribute** to assign a tab name.

{% code-snippet { file-name: ~Extension\Panels\PanelsAddIn.*, regions: [ModelViewTab] } %}
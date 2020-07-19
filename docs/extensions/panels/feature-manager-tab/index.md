---
title: Adding custom Windows Forms and WPF control into SOLIDWORKS Feature Manager using xCAD
caption: Feature Manager Tab
description: Instructions of how to add custom WPF and Windows Forms controls into the SOLIDWORKS Feature Manager using xCAD framework
image: feat-manager-view.png
---
![Custom Feature Manager Tab](feat-manager-view.png)

xCAD framework allows to add custom [Windows Forms controls](https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.usercontrol) and [WPF controls](https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.usercontrol) into Feature Manager tabs.

This functionality is only available for Part and Assembly documents

Decorate the control classes with **TitleAttribute** and **IconAttribute** to assign a tab tooltip and icon.

{% code-snippet { file-name: ~Extension\Panels\PanelsAddIn.*, regions: [FeatMgrTab] } %}
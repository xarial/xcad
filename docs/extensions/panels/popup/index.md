---
title: Displaying Windows or WPF form popup in SOLIDWORKS using xCAD
caption: Popup
description: Instructions of how to show custom Windows or WPF form popup in SOLIDWORKS using xCAD framework
image: winform-popup.png
---
![Windows Form Popup](winform-popup.png)

xCAD framework allows to show custom [Windows Form](https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.form) and [WPF Window](https://docs.microsoft.com/en-us/dotnet/api/system.windows.window) as a popup window.

![WPF Popup](wpf-popup.png)

Framework will automatically assign SOLIDWORKS window as a parent window for the forms.

{% code-snippet { file-name: ~Extension\Panels\PanelsAddIn.*, regions: [Popup] } %}
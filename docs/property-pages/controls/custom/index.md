---
title: Custom control (WPF or Windows Forms) in SOLIDWORKS property Manager Page
caption: Custom (WPF and Windows Forms)
description: Overview of options of custom controls (WPF and Windows Forms)
image: custom-wpf-control.png
order: 12
---
Custom control can be assigned to the property in the data model using the **CustomControlAttribute** and specifying the type of the control to render.

Both [Windows Forms controls](https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.usercontrol) and [WPF controls](https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.usercontrol) are supported.

## Hosting Windows Forms Control

![Windows Forms control hosted in the Property Manager Page](custom-winforms-control.png)

Create a property of any type which will represent data model bound to the control.

{% code-snippet { file-name: ~PropertyPage\Controls\CustomControl.*, regions: [WinForms] } %}

In order to properly associate data model with property manager page, it is required to implement the **IXCustomControl** interface in the windows forms control.

{% code-snippet { file-name: ~PropertyPage\Controls\CustomWinFormsControl.* } %}

Framework will bind **DataContext** property to the corresponding property in the data model of property manager page.

## Hosting WPF Control

![WPF control hosted in the Property Manager Page](custom-wpf-control.png)

Create a property of any type which will represent data model bound to the control.

{% code-snippet { file-name: ~PropertyPage\Controls\CustomControl.*, regions: [Wpf] } %}

The value of this property will be automatically assigned to the [FrameworkElemet::DataContext](https://docs.microsoft.com/en-us/dotnet/api/system.windows.frameworkelement.datacontext) property of the control. So it is possible to use WPF binding.

{% code-snippet { file-name: ~PropertyPage\Controls\CustomWpfControl.xaml } %}
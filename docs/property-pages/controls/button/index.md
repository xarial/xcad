---
title: Button control in SOLIDWORKS property Manager Page
caption: Button
description: Creating button control in the Property Manager Page using xCAD framework
image: button.png
order: 10
---
![Button control](button.png)

In order to create a button in the property manager page, it is required to declare the property of delegate type [Action](https://docs.microsoft.com/en-us/dotnet/api/system.action?view=netframework-4.8).

The pointer to void function assigned to this property is a handler of the button:

<<< @/_src/PropertyPage/Controls/Button.cs

Visit [bitmap button](../bitmap-button/index#button) for more information of how to create button with image.

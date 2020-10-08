---
title: Handling dynamic values updated in the controls
caption: Dynamic Values
description: Handling dynamic values updated in the controls of the Property Manager Page using xCAD framework
image: controls-dynamic-values.gif
order: 16
---
![Values updated controls](controls-dynamic-values.gif)

In order to update control values for the properties changed from the code behind dynamically (e.g. on button click or when one property is changing another property), it is required to implement the [INotifyPropertyChanged](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged?view=netframework-4.8) in the data model. Raise the [PropertyChanged](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged.propertychanged?view=netframework-4.8) event for every property which needs to be watched to notify the environment that value has changed and control needs to be updated.

{% code-snippet { file-name: ~PropertyPage\Controls\DynamicValues.* } %}

Refer [PMPageToggleBitmapButtons](https://github.com/xarial/xcad-examples/PMPageToggleBitmapButtons) example which demonstrates how to implement toggle bitmap buttons in the Property Manager Page

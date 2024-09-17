---
title: Creating native property pages with xCAD framework
caption: Property Pages
description: Utilities for advanced development of SOLIDWORKS Property Manager Pages which enables data driven development with data binding
image: data-model-pmpage.png
order: 4
---
Inspired by [PropertyGrid Control](https://msdn.microsoft.com/en-us/library/aa302326.aspx) in .NET Framework, xCAD brings the flexibility of data model driven User Interface into SOLIDWORKS API.

Framework allows to use data model structure as a driver of the User Interface. Framework will automatically generate required interface and implement the binding of the model.

This will greatly reduce the implementation time as well as make the property pages scalable, easily maintainable and extendable.

Property pages can be defined by data model and all controls will be automatically created and bound to the data.

![Property Manager Page driven by data model](data-model-pmpage.png){ width=250 }

Explore [Property Manager Page complete example](https://github.com/xarial/xcad-examples/tree/master/PMPage) for the source code of the features of the property manager pages.

## Data model

Start by defining the data model required to be filled by property manager page.

<<< @/_src/PropertyPage\Overview.cs#Simple

Use properties with public getters and setters

## Events handler

Create handler for property manager page by inheriting the public class from **Xarial.XCad.SolidWorks.UI.PropertyPage.SwPropertyManagerPageHandler** class.

This class will be instantiated by the framework and will allow handling the property manager specific events from the add-in.

<<< @/_src/PropertyPage\Overview.cs#PMPageHandler

> Class must be com visible and have public parameterless constructor.

Data model can directly inherit the handler.

## Ignoring members

If it is required to exclude the members in the data model from control generation such members should be decorated with **Xarial.XCad.UI.PropertyPage.Attributes.ExcludeControlAttribute**

<<< @/_src/PropertyPage\Overview.cs#Ignore

## Creating instance

Create instance of the property manager page by passing the type of the handler and data model instance into the generic arguments

> Data model can contain predefined (default) values. Framework will automatically use this values in the corresponding controls.

<<< @/_src/PropertyPage\Overview.cs#CreateInstance

> Store instance of the data model and the property page in the class variables. This will allow to reuse the data model in the different page instances.
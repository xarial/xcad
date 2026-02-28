---
title: Combo Box control in SOLIDWORKS property Manager Page
caption: Combo Box
description: Overview of options applied to Combo Box control
image: combobox.png
order: 7
---
![Combo Box control with 3 options](combobox.png)

Combo box control will be automatically generated for all the properties of enumerator types. All values of enumerators will be considered as the items in the combo box:

<<< @/_src/PropertyPage/Controls/ComboBox.cs#Simple

Additional options and style for combo box control can be specified via **ComboBoxOptionsAttribute**

## Item Text

**TitleAttribute** attribute can be used to specify user friendly title for the items to be shown in the combo box

<<< @/_src/PropertyPage/Controls/ComboBox.cs#ItemsText

## Dynamic Items Provider

In some cases items might need to be composed at runtime. In order to dynamically assign the list of item to combobox decorate the property with **CustomItemsAttribute**, create a type which implements **ICustomItemsProvider** (or **SwCustomItemsProvider{TItem}** for specific SOLIDWORKS implementation) and pass the type to attribute.

Framework will call the provider to resolve the items. Make sure that the type of the target property matches the type of values returned from the provider.

When returning custom class from **ProvideItems** override **ToString** method to provide display name for the item in the combo box.

<<< @/_src/PropertyPage/Controls/ComboBox.cs#CustomItemsProvider

In order to assign the control dependencies (i.e. controls which affect the list of values in the combobox), provide the corresponding control tags in the second parameter of **CustomItemsAttribute**. IN this case the **ProvideItems** method will be called when values of parent control change. In this case controls wil lbe passed as **dependencies** parameter of **ProvideItems**.

> Note. **dependencies** parameter of **ProvideItems** method will contain null items for the first rendering of the control before the binding is done. This method will be called again once binding is resolved with correct controls.

<<< @/_src/PropertyPage/Controls/ComboBox.cs#CustomItemsProviderDependency

Refer [Weldment Profiles Selector](https://github.com/xarial/xcad-examples/tree/master/WeldmentProfilesSelector) example which demonstrates how to create dynamic cascading combo boxes.
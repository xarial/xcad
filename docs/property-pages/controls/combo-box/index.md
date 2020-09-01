---
title: Combo Box control in SOLIDWORKS property Manager Page
caption: Combo Box
description: Overview of options applied to Combo Box control
image: combobox.png
order: 7
---
![Combo Box control with 3 options](combobox.png)

Combo box control will be automatically generated for all the properties of enumerator types. All values of enumerators will be considered as the items in the combo box:

{% code-snippet { file-name: ~PropertyPage\Controls\ComboBox.*, regions: [Simple] } %}

Additional options and style for combo box control can be specified via **ComboBoxOptionsAttribute**

## Item Text

**TitleAttribute** attribute can be used to specify user friendly title for the items to be shown in the combo box

{% code-snippet { file-name: ~PropertyPage\Controls\ComboBox.*, regions: [ItemsText] } %}

## Dynamic Items Provider

In some cases items might need to be composed at runtime. In order to dynamically assign the list of item to combobox decorate the property with **CustomItemsAttribute**, create a type which implements **ICustomItemsProvider** (or **SwCustomItemsProvider{TItem}** for specific SOLIDWORKS implementation) and pass the type to attribute.

Framework will call the provider to resolve the items. Make sure that the type of the target property matches the type of values returned from the provider.

{% code-snippet { file-name: ~PropertyPage\Controls\ComboBox.*, regions: [CustomItemsProvider] } %}
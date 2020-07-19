---
title: Selection box control in SOLIDWORKS Property Page with xCAD framework
caption: Selection Box
description: Overview of options applied to Selection Box control
image: selection-box.png
order: 8
---
![Selection box control](selection-box.png)

Selection box will be generated for the public properties of **IXSelObject**

{% code-snippet { file-name: ~PropertyPage\Controls\SelectionBox.*, regions: [Single] } %}

## Multiple Selection

This attribute is also applicable to lists. In this case multiple selections will be enabled for the selection box:

![Multiple entities selected in the selection box](selection-box-multiple.png)

{% code-snippet { file-name: ~PropertyPage\Controls\SelectionBox.*, regions: [List] } %}

Additional selection box options can be specified via **SelectionBoxOptionsAttribute**

## Selection Marks

Selection marks are used to differentiate the selection in the selection boxes. In most cases it is required for each selection to come into the specific selection box. In this case it is required to use different selection mark for every selection box. Selection marks are bitmasks, which means that they should be incremented with a power of two (i.e. 1, 2, 4, 8, 16 etc.) in order to be unique. By default xCAD framework will take care of assigning the correct selection marks. However it is possible to manually assign the marks using the corresponding overload of **SelectionBoxOptionsAttribute** constructors.

## Custom selection filters

To provide custom filtering logic for selection box it is required to implement the filter by inheriting the **ISelectionCustomFilter** interface and assign the filter via overloaded constructor of **SelectionBoxAttribute** attribute

{% code-snippet { file-name: ~PropertyPage\Controls\SelectionBox.*, regions: [CustomFilter] } %}
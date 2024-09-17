---
title: Edit SOLIDWORKS macro feature definition
caption: Edit Definition
description: Edit definition of SOLIDWORKS macro feature using xCAD framework
order: 2
---
Edit definition allows to modify the parameters of an existing feature. Edit definition is called when *Edit Feature* command is clicked form the feature manager tree.

![Edit Feature Command](menu-edit-feature.png){ width=250 }

Use **ISwMacroFeature``<TParams>``.Parameters** property to read and write the parameters of this macro feature. Set the value to **null** to revert changes and rollback the feature.

<<< @/_src/CustomFeature/EditMacroFeatureDefinition.cs
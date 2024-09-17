---
title: Storing data (parameters, bodies, selection) in SOLIDWORKS macro feature
caption: Data
description: Storing the parameters, metadata, dimensions, selections in the SOLIDWORKS macro feature using xCAD framework
order: 3
---
Macro feature can store additional metadata and entities. The data includes

* Parameters
* Selections
* Edit bodies
* Dimensions

Required data can be defined within the macro feature data model. Special parameters (such as selections, edit bodies or dimensions) should be decorated with appropriate attributes, all other properties will be considered as parameters.

Data model is used both as input and output of macro feature. Parameters can be accessed via **SwMacroFeature``<TParams>``.Parameters** property and also passed to **OnRebuild** handler.

<<< @/_src/CustomFeature\MacroFeatureParameters.cs
---
title: Managing selection of SOLIDWORKS macro feature
caption: Selections
description: Managing selections of SOLIDWORKS macro feature using the xCAD framework
order: 2
---
<<< @/_src/CustomFeature/MacroFeatureSelectionParams.cs

Parameters of **IXSelObject** will be recognized as selection objects and stored appropriately in macro feature.

**OnRebuild** handler will be called if any of the selections have changed.
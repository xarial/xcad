---
title: Group Box in SOLIDWORKS Property Manager Page
caption: Group Box
description: Overview of functionality to groups the controls
image: group-box.png
order: 3
---
![Group box created from the complex type](group-box.png)

Group box will be automatically created for all complex types in the data model

<<< @/_src/PropertyPage/Controls/Group.cs

> SOLIDWORKS doesn't support groups nested into other groups, so all the nested complex types will be added as the groups to the main property manager page.

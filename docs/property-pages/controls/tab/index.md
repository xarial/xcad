---
title: Tab control in SOLIDWORKS property Manager Page
caption: Tab
description: Creating tab control in the Property Manager Page using xCAD framework
image: pmpage-tab.png
order: 3
---
![Controls grouped in Property Manager Page tabs](pmpage-tab.png)

Tab containers are created for the complex types decorated with **TabAttribute**.

<<< @/_src/PropertyPage/Controls/Tab.cs

## Tab with nested groups

Controls can be added directly to tabs or can reside in the nested groups:

<<< @/_src/PropertyPage/Controls/Tab.cs#WithGroup

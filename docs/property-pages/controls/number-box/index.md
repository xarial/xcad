---
title: Number Box in SOLIDWORKS Property Manager Page
caption: Number Box
description: Overview of options applied to Number Box control
image: number-box-units-wheel.png
order: 6
---
![Simple number box](number-box.png)

Number box will be automatically created for the properties of *int* and *double* types.

{% code-snippet { file-name: ~PropertyPage\Controls\NumberBox.*, regions: [Simple] } %}

Style of the number box can be customized via the **NumberBoxOptionsAttribute**

![Number boxes with additional styles allowing specifying the units and displaying thumbwheel for changing the value](number-box-units-wheel.png)

{% code-snippet { file-name: ~PropertyPage\Controls\NumberBox.*, regions: [Style] } %}

#REMARK on Number box units. 
Use 0 for not using units. 
If units are specified then the corresponding current user unit system will be used and the corresponding units marks will be displayed in the number box. 
Regardless of the current unit system the value  will be stored in system units (MKS)

![image](https://user-images.githubusercontent.com/979361/132507448-2858fdb2-a1b6-4f29-aa16-57c3fc9a5563.png)


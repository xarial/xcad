---
title: Common Options of all controls in SOLIDWORKS property manager page
caption: Common Options
description: Overview of options applied to all controls in the SOLIDWORKS property manager page
image: property-manager-page-control.png
order: 1
---
All generated controls have common properties which can be customized

![Control common properties](property-manager-page-control.png)

1. Control icon selected from the standard library of icons
1. Custom control icon loaded from the image
1. Tooltip of the control displayed on mouse hovering

## Style

Common styles can be customized via **ControlOptionsAttribute** by decorating the specific properties in data model.

This attribute allows to define the alignment, position, size as well as background and foreground colours:

<<< @/_src/PropertyPage/Controls/CommonOptions.cs#Style

![Custom background and foreground colours applied to textbox](textbox-foreground-background.png)

## Attribution

### Tooltip

Tooltip for controls can be set by applying the [DescriptionAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.descriptionattribute?view=netframework-4.0)

### Standard Icon

![Standard icon added to text box control](standard-icon-textbox.png)

Standard icon defined in **BitmapLabelType_e** enumeration can be set to control via **StandardControlIconAttribute** attribute.

<<< @/_src/PropertyPage/Controls/CommonOptions.cs#StandardIcon

Use the below map of all available standard icons:

![Standard bitmap icons for Property Manager Page controls](property-page-controls-standard-icons.png)

1. LinearDistance
1. AngularDistance
1. SelectEdgeFaceVertex
1. SelectFaceSurface
1. SelectVertex
1. SelectFace
1. SelectEdge
1. SelectFaceEdge
1. SelectComponent
1. Diameter
1. Radius
1. LinearDistance1
1. LinearDistance2
1. Thickness1
1. Thickness2
1. LinearPattern
1. CircularPattern
1. Width
1. Depth
1. KFactor
1. BendAllowance
1. BendDeduction
1. RipGap
1. SelectProfile
1. SelectBoundary

### Custom Icon

Custom icon can be set via overloaded constructor of **IconAttribute** attribute

<<< @/_src/PropertyPage/Controls/CommonOptions.cs#CustomIcon
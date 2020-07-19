---
title: Managing dimensions in the SOLIDWORKS macro feature using xCAD framework
caption: Dimensions
description: Adding dimensions (linear and radial) to the SOLIDWORKS macro feature using xCAD framework
order: 4
---
Dimensions is an additional source of input for macro feature. Dimensions can be defined in the following way:

{% code-snippet { file-name: ~CustomFeature\DimensionsParameters.* } %}

It is required to arrange the dimensions within rebuild by specifying the *alignDim* delegate. Use **IXCustomFeatureDefinition<TParams>.AlignDimension** and extension helper methods to align the dimension.

{% code-snippet { file-name: ~CustomFeature\SetDimensions.* } %}

*Origin* is a starting point of the dimension.

For linear dimensions *orientation* represents the vector along the direction of the dimension (i.e. the direction of measured entity)
For radial dimensions *orientation* represents the normal of the dimension (i.e. the vector of rotation of the dimension)

![Orientation of dimensions](dimensions-orientation.png){ width=350 }
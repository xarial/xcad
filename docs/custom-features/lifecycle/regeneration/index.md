---
title: Handling Regeneration method of SOLIDWORKS macro feature
caption: Regeneration
description: Handling regeneration event of SOLIDWORKS macro feature and returning bodies or errors to drive the behavior using xCAD framework
order: 1
---
This handler called when feature is being rebuilt (either when regenerate is invoked or when the parent elements have been changed).

Use **CustomFeatureRebuildResult** class to generate the required output.

Feature can generate the following output

{% code-snippet { file-name: ~CustomFeature\RegenerationResults.* } %}

Use **IXGeometryBuilder** interface if feature needs to create new bodies. Only temp bodies can be returned from the regeneration method.
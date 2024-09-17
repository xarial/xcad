---
title: Backward compatibility support for SOLIDWORKS macro feature parameters
caption: Backward Compatibility
description: Explanation of ways to implement backward compatibility for the parameters stored in SOLIDWORKS macro feature
order: 5
---
## Parameters

Macro feature parameters might need to change from version to version. And xCAD framework provides a mechanism to handle the backward compatibility of existing features.

Mark current version of parameters with **ParametersVersionAttribute** and increase the version if any of the parameters changed.

Implement the **ParametersVersionConverter** to convert from the latest version of the parameters to the newest one. Framework will take care of aligning versions in case parameters are older than one version.

Old version of parameters

<<< @/_src/CustomFeature\BackwardCompatibility.cs#OldParams

New version of parameters

<<< @/_src/CustomFeature\BackwardCompatibility.cs#NewParams

Converter between version 1 and 2 can be implemented in the following way:

<<< @/_src/CustomFeature\BackwardCompatibility.cs#Converter
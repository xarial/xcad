---
title: Managing of Edit Bodies in SOLIDWORKS macro feature
caption: Edit Bodies
description: Managing of Edit Bodies in SOLIDWORKS macro feature using xCAD framework
order: 3
---
Edit bodies are input bodies which macro feature will acquire. For example when boss-extrude feature is created using the merge bodies option the solid body it is based on became a body of the new boss-extrude. This could be validated by selecting the feature in the tree which will select the body as well. In this case the original body was passed as an edit body to the boss-extrude feature.

<<< @/_src/CustomFeature/EditBodies.cs#single

If multiple input bodies are required it could be either specified in different properties

<<< @/_src/CustomFeature/EditBodies.cs#multiple

or as list

<<< @/_src/CustomFeature/EditBodies.cs#list

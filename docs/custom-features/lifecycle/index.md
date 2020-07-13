---
title: Life cycle of SOLIDWORKS macro feature
caption: Lifecycle
description: Explanation of the SOLIDWORKS macro feature behavior and life cycle
order: 2
---
Macro feature resides in the model and saved together with the document. Macro feature can handle various events during its lifecycle

* Regeneration. Override **OnRebuild** method to handle this event.
* Editing. Override **OnEditDefinition** method to handle this event.
* Updating state. Override **OnUpdateState** method to handle this event.

Macro feature is a singleton service. Do not create any class level variables in the macro feature class.
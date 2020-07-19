---
title: Handling the common events of SOLIDWORKS file using xCAD framework
caption: Common Events
description: Handling of common events (rebuild, selection, configuration change, item modification, custom property modification etc.) using documents management functionality in xCAD Framework
labels: [events,rebuild,selection]
---
xCAD framework exposes the common events via corresponding interfaces, e.g. **IXProperty** exposes **ValueChanged** event to indicate that the property has been changed, while **IXSelectionRepository** exposes **NewSelection** event to indicate that new object is selected.

Although it is possible to subscribe to events from any container, it is usually managed within the **IDocumentHandler**

{% code-snippet { file-name: ~EventsAddIn.*, regions: [RegisterHandler] } %}

Explore API reference for more information about the passed parameters.

{% code-snippet { file-name: ~EventsAddIn.*, regions: [EventHandlers] } %}
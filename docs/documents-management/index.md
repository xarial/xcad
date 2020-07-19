---
title: Managing SOLIDWORKS documents life cycle via xCAD framework
caption: Documents Management
description: Framework to manage SOLIDWORKS documents life cycle (open, close, activate) and its events in xCAD
order: 3
---
xCAD frameworks provides utility class to manage document life cycle by creating a specified instance handler as a wrapper of a model.

Call **IXDocumentCollection::RegisterHandler** method and pass the type of document handler as a generic argument. Handle [common events](events/) (e.g. saving, selection, rebuilding, [3rd party storage access](/third-party-data-storage/)) or specific event within the handler implementation.

{% code-snippet { file-name: ~DocMgrAddIn.*, regions: [DocHandlerInit] } %}

Define the document handler either by implementing the **IDocumentHandler** interface or **SwDocumentHandler** class. 

{% code-snippet { file-name: ~DocMgrAddIn.*, regions: [DocHandlerDefinition] } %}

Override methods of document handler and implement required functionality attached for each specific SOLIDWORKS model (such as handle events, load, write data etc.)

Framework will automatically dispose the handler. Unsubscribe from the custom events within the **Dispose** method. The pointer to the document attached to the handler is assigned to **Model** property of **SwDocumentHandler**.
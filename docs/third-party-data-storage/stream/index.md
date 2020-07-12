---
title: Storing data in the 3rd party storage (stream) via SwEx.AddIn framework
caption: Stream
description: Serializing custom structures into the 3rd party storage (stream) using SwEx.AddIn framework
order: 1
---
Call **IXDocument::OpenStream** method to access the 3rd party stream. Pass the access parameter to read or write stream.

Use this approach when it is required to store a single structure at the model.

## Stream Access Handler

To simplify the handling of the stream lifecycle, use the Documents Manager API from the SwEx.AddIn framework:

{% code-snippet { file-name: ~ThirdPartyData.*, regions: [StreamHandler] } %}

## Reading data

**IXDocument::OpenStream** method throws an exception when storage does not exist. Use **IXDocument::TryOpenStream** extension method which returns null for the storage which not exists on reading.

{% code-snippet { file-name: ~ThirdPartyData.*, regions: [StreamLoad] } %}

## Writing data

**IXDocument::OpenStream** will always return the pointer to the stream (stream is automatically created if it doesn't exist).

{% code-snippet { file-name: ~ThirdPartyData.*, regions: [StreamSave] } %}

---
title: Storing data in the 3rd party storage store via xCAD framework
caption: Storage
description: Serializing custom structures into the 3rd party storage store using xCAD framework
order: 2
---
Call **IXDocument::OpenStorage** method to access the 3rd storage store. Pass the access parameter to read or write storage.

Use this approach when it is required to store multiple data structures which need to be accessed and managed independently. Prefer this instead of creating multiple [streams](/third-party-data-storage/stream/)

## Storage Access Handler

To simplify the handling of the storage lifecycle, use the Documents Manager API from the xCAD framework:

<<< @/_src/ThirdPartyData.cs#StorageHandler

## Reading data

**IXDocument::OpenStorage** method throws an exception when storage does not exist. Use **IXDocument::TryOpenStorage** extension method which returns null for the storage which not exists on reading.

<<< @/_src/ThirdPartyData.cs#StorageLoad

## Writing data

**IXDocument::OpenStorage** method will always return the pointer to the storage (stream is automatically created if it doesn't exist).

<<< @/_src/ThirdPartyData.cs#StorageSave

Explore the methods of **IStorage** for information of how to create sub streams or sub storages and enumerate the existing elements.
---
title: SOLIDWORKS Property Manager Page closing events handling
caption: Closing
description: Overview of events associated with closing of SOLIDWORKS property manager page handled in xCAD framework
order: 1
---
## Pre Closing event

**Closing** event is raised when property manager page is about to be closed.

Framework passes the reason of close and **closing argument** which allows to cancel property manager page closing and display error to the user as a tooltip.

<<< @/_src/PropertyPage/Events.cs#Closing

This event is raised when Property Manager Page dialog is still visible. There should be no rebuild operations performed within this handler, it includes the direct rebuilds but also any new features or geometry creation or modification (with an exception of temp bodies). Note that some operations such as saving may also be unsupported. In general if certain operation cannot be performed from the user interface while property page is opened it shouldn't be called from the closing event via API as well. Otherwise this could cause instability including crashes. Use [Post closing event](#post-closing-event) event to perform any rebuild operations.

In some cases it is required to perform this operation while property manager page stays open. Usually this happens when page supports pining (PushpinButton flag of **PageOptions_e** enumeration in **PageOptionsAttribute**. In this case it is required to set the LockedPage flag of **PageOptions_e** enumeration in **PageOptionsAttribute**. This would enable the support of rebuild operations and feature creation from within the **SwPropertyManagerPage::Closing** event.

## Post closing event

**Closed** event is raised when property manager page is closed.

Use this handler to perform the required operations.

<<< @/_src/PropertyPage/Events.cs#Closed
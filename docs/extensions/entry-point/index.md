---
title: Entry Point of xCAD framework
caption: Entry Point
description: Instructions on starting the coding with the xCAD framework for SOLIDWORKS
order: 2
---
## OnConnect

<<< @/_src/Extension/EntryPoint.cs#Connect

This function is called within the ConnectToSw entry point. Override this function to initialize the add-in.

Thrown an exception to indicate that the initialization is unsuccessful. This will cancel the loading of the add-in.

This override should be used to validate license (return false if the validation is failed), add command manager, task pane views, initialize events manager, etc.

## OnDisconnect

<<< @/_src/Extension/EntryPoint.cs#Disconnect

This function is called within the DisconnectFromSw function. Use the function to release all resources. You do not need to release the com pointers to SOLIDWORKS or command manager as those will be automatically released by xCAD framework.

## Accessing SOLIDWORKS application objects

xCAD framework provides the access to the SOLIDWORKS specific add-in data and objects which are preassigned by the framework. This includes pointer to SOLIDWORKS application, add-in id, pointer to command manager.

<<< @/_src/Extension/EntryPoint.cs#SwObjects
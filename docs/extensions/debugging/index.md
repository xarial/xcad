---
title: Debugging SOLIDWORKS add-ins with xCAD.NET
caption: Debugging
description: Instructions on debugging add-ins for SOLIDWORKS developed using xCAD.NET
order: 4
---
SOLIDWORKS add-ins are in-process applications hosted within the **sldworks.exe** process.

When debugging SOLIDWORKS add-ins it is recommended to specify the full path to the SOLIDWORKS executable for the **Start external program** option in the project settings under the **Debug** tab.

![Setting path to SOLIDWORKS as external program](start-externel-process.png){ width=600 }

In this case it is possible to launch SOLIDWORKS and attach to the process automatically directly form Visual Studio by calling **Start** command or clicking **F5**

To attach to the running SOLIDWORKS instance use the **Debug->Attach To Process...** command

![Attach to running process](attach-to-process.png)

and select **SLDWORKS.exe** process form the list

![Attaching to SLDWORKS.exe process](sldworks-process.png){ width=600 }
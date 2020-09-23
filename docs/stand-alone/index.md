---
title: Using xCAD to create out-of-process (stand-alone) applications
caption: Stand-Alone
description: Creating out-of-process (stand-alone) executable applications (console, win forms, WPF application)
order: 3
---
xCAD framework can be used to create out-of-process (stand-alone) applications, such as Console, Windows Forms, WPF etc. in .NET Framework or .NET Core.

{% youtube id: 0ubF-INE7bg %}

Call **SwApplication.Start** to connect to SOLIDWORKS instance in one of the following ways:

* To the specified SOLIDWORKS version
* To the latest SOLIDWORKS version (set the value of *vers* parameter to null)
* By optionally providing additional arguments

In order to connect to existing (running process of SOLIDWORKS) use **SwApplication.FromProcess** method and pass the pointer to [Process](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process?view=netcore-3.1)

{% code-snippet { file-name: ~StandAlone.* } %}

Refer [Console Model Generator](https://github.com/xarial/xcad-examples/tree/master/ModelGeneratorConsole) example which demonstrates how to access xCAD.API from .NET Core console.
---
title: Deployment of xCAD add-ins
caption: Deployment
description: Deployment guidelines for SOLIDWORKS add-ins developed with xCAD framework
order: 7
---
Add-ins developed with xCAD can be deployed in exact same way as any other add-ins either via manual registration or by developing an installer package (.msi).

## Manual Registration

SOLIDWORKS add-ins are COM classes and required to be registered on a target machine as COM classes.

Command can be executed from the Windows Command Line.

> Administrative permissions might be required for registering of COM object.

Registration command differs depending on if the add-in developed using .NET Framework or .NET Core.

### .NET Framework

~~~
> %windir%\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe "Full Path To Add-in dll"
~~~

To unregister add-in use the following command:

~~~
> %windir%\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe "Full Path To Add-in dll" /u
~~~

### .NET Core

~~~
> regsvr32 "Full Path To Add-in .comhost.dll"
~~~

To unregister add-in use the following command:

~~~
> regsvr32 "Full Path To Add-in .comhost.dll" /u
~~~

If **Xarial.XCad.Extensions.Attributes.SkipRegistrationAttribute** is used then all the required registry information will not be added and it is additionally required to add the registry keys when registering the add-in by running the following .reg file, where

* ADDIN_GUID - GUID of the add-in
* ADDIN_TITLE - User friendly name of the add-in
* ADDIN_DESCRIPTION - Summary of the add-in

~~~
Windows Registry Editor Version 5.00

[HKEY_LOCAL_MACHINE\SOFTWARE\SolidWorks\AddIns\{ADDIN_GUID}]
@=dword:00000000
"Description"="ADDIN_DESCRIPTION"
"Title"="ADDIN_TITLE"

[HKEY_CURRENT_USER\Software\SolidWorks\AddInsStartup\{ADDIN_GUID}]
@=dword:00000001
~~~

> If the **Xarial.XCad.Extensions.Attributes.SkipRegistrationAttribute** attribute is not set explicitly, registry information will be added automatically upon registration and it is not required to run the .reg file

Follow [Installing the SOLIDWORKS add-in by creating the msi-installer](https://www.codestack.net/solidworks-api/deployment/installer/) for detailed instructions of how to create an .msi installer package for the add-in.
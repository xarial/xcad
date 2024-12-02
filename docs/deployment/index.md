---
title: How to deploy add-ins created with xCAD.NET
caption: Deployment & Installers
description: Creating deployment scripts and msi-installers for SOLIDWORKS add-ins created with xCAD.NET
order: 8
---
xCAD.NET automatically registers SOLIDWORKS add-in when add-in dll is registered with Regasm tool or build from Microsoft Visual Studio (unless explicitly disabled in the project settings). It is not required to additionally specify build event actions or add registry keys.

## Manual Deployment

The simplest way to deploy SOLIDWORKS add-in to other computer is to copy the output of bin directory to the target machine and run regasm tool with **/codebase** switch:

> It might be required to run this script with administrative previledges
::: code-group

``` [Install]
"%Windir%\Microsoft.NET\Framework64\v4.0.30319\regasm" /codebase "PATH TO ADDIN DLL"
```

``` [Uninstall]
"%Windir%\Microsoft.NET\Framework64\v4.0.30319\regasm" /codebase "PATH TO ADDIN DLL" /u
```
:::

### Adding Command Line Scripts

The process can be improved by creating the .cmd script which can be placed in the output directory. In this case it will be simply required to execute this script from the Windows File Explore.

![Run register.cmd script](run-register-cmd.png)

Content of:

::: code-group

``` [**register.cmd** file]
"%windir%\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe" /codebase "%~dp0[NAME OF THE ADDIN FILE].dll"
```

``` [**unregister.cmd** file]
"%windir%\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe" /codebase "%~dp0[NAME OF THE ADDIN FILE].dll" /u
```

:::

## Creating MSI Installers

For more advanced experience MSI-installers can be created. This will enable an installation wizard and allow admin image installation. Product will be availble in Windows Control panel and can be repaired or uninstalled.

There are multiple solutions (free and paid) available. 

### Visual Studio Installer (VSI)

### Windows Installer XML (WiX)

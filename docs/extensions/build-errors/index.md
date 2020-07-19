---
title: Troubleshooting build errors for add-ins in xCAD
caption: Build Errors
description: Troubleshooting techniques for registration and use of the xCAD.NET framework
order: 3
---
## Insufficient Permissions

### Symptoms

*Requested registry access is not allowed*

![regasm error when building an add-in](regasm-error.png)

or *The command 'regsvr32' exited with code 5*

![regsvr32 error when building an add-in](regsvr32-error.png)

error is displayed during the build process.

### Cause

Registration may require administrative privileges and IDE (e.g. Visual Studio or Visual Studio Code) is not run *As Administrator*

### Resolution

* Run IDE As Administrator
* If the above does not help, try deleting the bin folder, in some cases it can resolve the issue
* Alternatively, [disable](/extensions/registering/) automatic registration and register add-in manually from the command line

## Embedded SOLIDWORKS Interops

### Symptoms

*Could not load file or assembly 'SolidWorks.Interop.Published'* error is displayed when building or cleaning project.

![Build error due to interop libraries being embedded](embed-interops-error.png)

### Cause

xCAD (when used in .NET Framework) requires the the SOLIDWORKS interop files to be available in the output directory for the proper loading.

xCAD automatically sets the *Embed Interop Types* option to *False* for all necessarily libraries when nuget package is installed. However in some cases (upgrade to package or specific version of Nuget package manager) this may result into the libraries to be set as embedded.

### Resolution

Manually change the *Embed Interop Types* option to *False* for the SOLIDWORKS interops.

![Embed Interop Types option set to False](embed-interop-types.png)
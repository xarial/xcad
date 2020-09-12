---
title: Registering SOLIDWORKS add-in using xCAD
caption: Registration
description: Registering .NET Framework and .NET Core add-ins using xCAD (automatic or manual option)
order: 1
---
xCAD framework will automatically register the add-in by performing 2 steps (no need to run custom regasm commands, no need to call any static classes):

* Registering the assembly as COM via regasm for .NET Framework add-ins and regsvr32 for .NET Core add-ins. It is however possible to disable this behavior by adding the *XCadRegDll* property into the .csproj *PropertyGroup*. In this case you can manually register the add-in via command line or post build actions.

~~~ xml jagged
<PropertyGroup>
    <XCadRegDll>false</XCadRegDll>
</PropertyGroup>
~~~

* Adding the required parameters to the Windows Registry. To skip an automatic registration decorate the add-in class with **Xarial.XCad.Extensions.Attributes.SkipRegistrationAttribute**.

{% code-snippet { file-name: ~Extension\Register.*, regions: [SkipReg] } %}

> It might be required to run Visual Studio As Administrator to allow the registration of COM object and adding the keys to registry.

## .NET Framework

To define add-in just add the [ComVisibleAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.comvisibleattribute?view=netframework-4.8).

Although it is not a essential requirement, it is recommended to assign the GUID to the add-in class via [GuidAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.guidattribute?view=netcore-3.1).

{% code-snippet { file-name: ~Extension\Register.*, regions: [NetFramework] } %}

## .NET Core

Unlike .NET Framework registration, COM class must be decorated with [GuidAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.guidattribute?view=netcore-3.1).

In addition, it is required to add the *EnableComHosting* property into the *.csproj file and explicitly call the registration due to known limitation of .NET Core as shown below:

~~~ xml jagged
<PropertyGroup>
    <EnableComHosting>true</EnableComHosting>
</PropertyGroup>
~~~

{% code-snippet { file-name: ~Extension\Register.*, regions: [NetCore] } %}

It is also required to change the SDK of the add-in project to *Microsoft.NET.Sdk.WindowsDesktop* and set the *UseWindowsForms* attribute. This would enable the support for resources and other windows specific .NET classes used by framework.

~~~ xml jagged-bottom
<Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">

  <PropertyGroup>
    <UseWindowsForms>true</UseWindowsForms>
  </PropertyGroup>
~~~

> Note, .NET Core is a new framework and there were some compatibility issues and conflicts reported with other 3rd party libraries when running as in-process application (i.e. add-in). It is recommended to use .NET Framework for add-ins development where possible until .NET Core is fully supported by SOLIDWORKS host application.

## Unregistering add-in

Add-in will be automatically removed and all COM objects unregistered when project is cleaned in Visual Studio
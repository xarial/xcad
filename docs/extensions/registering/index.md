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

~~~ cs jagged-bottom
[ComVisible(true)]
[Xarial.XCad.Extensions.Attributes.SkipRegistration]
public class SampleAddIn : SwAddInEx
{
~~~

## .NET Framework

To define add-in just add the [ComVisibleAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.comvisibleattribute?view=netframework-4.8).

Although it is not a essential requirement, it is recommended to assign the GUID to the add-in class via [GuidAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.guidattribute?view=netcore-3.1).

~~~ cs
[ComVisible(true)]
public class SampleAddIn : SwAddInEx
{
    public override void OnConnect()
    {
    }
}
~~~

## .NET Core

Unlike .NET Framework registration, COM class must be decorated with [GuidAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.guidattribute?view=netcore-3.1).

In addition, it is required to add the *EnableComHosting* property into the *.csproj file and explicitly call the registration due to known limitation of .NET Core as shown below:

~~~ xml jagged
<PropertyGroup>
    <EnableComHosting>true</EnableComHosting>
</PropertyGroup>
~~~

~~~ cs
[ComVisible(true), Guid("612378E1-C962-468C-9810-AF5AE1245EB7")]
public class SampleAddIn : SwAddInEx
{
    [ComRegisterFunction]
    public static void RegisterFunction(Type t)
    {
        SwAddInEx.RegisterFunction(t);
    }

    [ComUnregisterFunction]
    public static void UnregisterFunction(Type t)
    {
        SwAddInEx.UnregisterFunction(t);
    }

    public override void OnConnect()
    {
    }
}
~~~

It is also recommended to change the SDK of the add-in project to *Microsoft.NET.Sdk.WindowsDesktop* and set the *UseWindowsForms* attribute. This would enable the support for resources and other windows specific .NET classes used by framework.

~~~ xml jagged-bottom
<Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">

  <PropertyGroup>
    <UseWindowsForms>true</UseWindowsForms>
  </PropertyGroup>
~~~
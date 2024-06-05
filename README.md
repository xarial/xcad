# xCAD.NET: SOLIDWORKS API development made easy

![Logo](https://raw.githubusercontent.com/xarial/xcad/master/data/icon.png)

[![NuGet version (xCAD.NET)](https://img.shields.io/nuget/v/Xarial.XCad.svg?style=flat-square)](https://www.nuget.org/packages/Xarial.XCad/)
[![Build status](https://dev.azure.com/xarial/xcad/_apis/build/status/xcad)](https://dev.azure.com/xarial/xcad/_build/latest?definitionId=34)

[![Templates](https://img.shields.io/badge/-Templates-yellow.svg)](https://www.nuget.org/packages/Xarial.XCad.Templates/)
[![User Guide](https://img.shields.io/badge/-Documentation-green.svg)](https://xcad.xarial.com)
[![Examples](https://img.shields.io/badge/-Examples-blue.svg)](https://github.com/xarial/xcad-examples)
[![Videos](https://img.shields.io/badge/-Videos-red.svg)](https://www.youtube.com/watch?v=YLFwqTX_V2I&list=PLZ8T-hyutVIEXMFgJ462Ou6Szjk26gPVo)

[xCAD.NET](https://xcad.net) is a framework for building CAD agnostic applications. It allows developers to implement complex functionality with a very simple innovative approach. This brings the best user experience to the consumers of the software.

## Templates

Visual Studio and Visual Studio Code templates can be installed from [NuGet](https://www.nuget.org/packages/Xarial.XCad.Templates/)

~~~
> dotnet new install Xarial.XCad.Templates
~~~

## SOLIDWORKS Add-in Applications

It has never been easier to create SOLIDWORKS add-ins with toolbar and menu commands.

~~~ cs
[ComVisible(true)]
public class XCadAddIn : SwAddInEx
{
    public enum Commands_e
    {
        Command1,
        Command2
    }

    public override void OnConnect()
    {
        this.CommandManager.AddCommandGroup<Commands_e>().CommandClick += OnCommandsButtonClick;
    }

    private void OnCommandsButtonClick(Commands_e cmd)
    {
        //TODO: handle the button click
    }
}
~~~

## Property Manager Pages

Framework reinvents the way you work with Property Manager Pages. No need to code a complex code behind for adding the controls and handling the values. Simply define your data model and the framework will build the suitable Property Manager Page automatically and two-way bind controls to the data model.

~~~ cs
[ComVisible(true)]
public class IntroPmpPageAddIn : SwAddInEx
{
    [ComVisible(true)]
    public class MyPMPageData : SwPropertyManagerPageHandler
    {
        public string Text { get; set; }
        public int Number { get; set; }
        public IXComponent Component { get; set; }
    }

    private enum Commands_e
    {
        ShowPmpPage
    }

    private IXPropertyPage<MyPMPageData> m_Page;
    private MyPMPageData m_Data = new MyPMPageData();

    public override void OnConnect()
    {
        m_Page = this.CreatePage<MyPMPageData>();
        m_Page.Closed += OnPageClosed;
        this.CommandManager.AddCommandGroup<Commands_e>().CommandClick += ShowPmpPage;
    }

    private void ShowPmpPage(Commands_e cmd)
    {
        m_Page.Show(m_Data);
    }

    private void OnPageClosed(PageCloseReasons_e reason)
    {
        Debug.Print($"Text: {m_Data.Text}");
        Debug.Print($"Number: {m_Data.Number}");
        Debug.Print($"Selection component name: {m_Data.Component.Name}");
    }
}
~~~

## Macro Features

Complex macro features became an ease with xCAD.NET

~~~ cs
[ComVisible(true)]
public class IntroMacroFeatureAddIn : SwAddInEx 
{
    [ComVisible(true)]
    public class BoxData : SwPropertyManagerPageHandler
    {
        public double Width { get; set; }
        public double Length { get; set; }
        public double Height { get; set; }
    }

    [ComVisible(true)]
    public class BoxMacroFeature : SwMacroFeatureDefinition<BoxData, BoxData>
    {
        public override ISwBody[] CreateGeometry(ISwApplication app, ISwDocument model, ISwMacroFeature<BoxData> feat)
        {
            var data = feat.Parameters;

            var body = (ISwBody)app.MemoryGeometryBuilder.CreateSolidBox(new Point(0, 0, 0),
                new Vector(1, 0, 0), new Vector(0, 1, 0),
                data.Width, data.Length, data.Height).Bodies.First();

            return new ISwBody[] { body };
        }

    }

    public enum Commands_e
    {
        InsertMacroFeature,
    }

    public override void OnConnect()
    {
        this.CommandManager.AddCommandGroup<Commands_e>().CommandClick += OnCommandsButtonClick;
    }

    private void OnCommandsButtonClick(Commands_e cmd)
    {
        switch (cmd) 
        {
            case Commands_e.InsertMacroFeature:
                Application.Documents.Active.Features.CreateCustomFeature<BoxMacroFeature, BoxData, BoxData>();
                break;
        }
    }
}
~~~

## SOLIDWORKS And Document Manager API

xCAD.NET allows to write the same code targeting different CAD implementation in a completely agnostic way. Example below demonstrates how to perform opening of assembly, traversing components recursively and closing the assembly via SOLIDWORKS API and [SOLIDWORKS Document Manager API](https://www.codestack.net/solidworks-document-manager-api/) using the same code base.

~~~ cs
static void Main(string[] args)
{
    var assmFilePath = @"C:\sample-assembly.sldasm";

    //print assembly components using SOLIDWORKS API
    var swApp = SwApplicationFactory.Create(SwVersion_e.Sw2022, ApplicationState_e.Silent);
    PrintAssemblyComponents(swApp, assmFilePath);

    //print assembly components using SOLIDWORKS Document Manager API
    var swDmApp = SwDmApplicationFactory.Create("[Document Manager Lincese Key]");
    PrintAssemblyComponents(swDmApp, assmFilePath);
}

//CAD-agnostic function to open assembly, print all components and close assembly
private static void PrintAssemblyComponents(IXApplication app, string filePath) 
{
    using (var assm = app.Documents.Open(filePath, DocumentState_e.ReadOnly))
    {
        IterateComponentsRecursively(((IXAssembly)assm).Configurations.Active.Components, 0);
    }
}

private static void IterateComponentsRecursively(IXComponentRepository compsRepo, int level) 
{
    foreach (var comp in compsRepo)
    {
        Console.WriteLine(Enumerable.Repeat("  ", level) + comp.Name);

        IterateComponentsRecursively(comp.Children, level + 1);
    }
}
~~~

## Target Frameworks

xCAD.NET is compatible with multiple target frameworks: .NET Framework 4.6.1, .NET Core 3.1, .NET 6.0, .NET 7.0 and number of additional computed target frameworks (e.g. .NET Framework 4.8)

When building the SOLIDWORKS add-ins see the information below

### .NET Framework

* Run Visual Studio as an Administrator
* Install [Xarial.XCad.SolidWorks](https://www.nuget.org/packages/Xarial.XCad.SolidWorks) package from the nuget and create add-in class as shown above
* Build the solution. Add-in will be automatically registered. Clean the solution to unregister the add-in.
* Set the **Embed Interop** option to **True** for all SOLIDWORKS type libraries (e.g. **SolidWorks.Interop.SldWorks.tlb**, **SolidWorks.Interop.SwConst.tlb**, **SolidWorks.Interop.SwPublished.tlb**). Note this might not be required as nuget will set this flag automatically.

### .NET Core/.NET 6/.NET 7

* Run Visual Studio as an Administrator
* Install [Xarial.XCad.SolidWorks](https://www.nuget.org/packages/Xarial.XCad.SolidWorks) package from the nuget and create add-in class as shown above
* Add the following property into the project file (*.csproj or *.vbproj)
~~~ xml
<PropertyGroup>
    <EnableComHosting>true</EnableComHosting>
</PropertyGroup>
~~~
* Build the solution. Add-in will be automatically registered. Clean the solution to unregister the add-in.

### .NET Core Only

Automatic registration does not work in .NET Core and it needs to be called manually by adding the following code into the add-in (this is not required for .NET6)

~~~ cs
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
~~~

Watch the [video demonstrations YouTube playlist](https://www.youtube.com/watch?v=YLFwqTX_V2I&list=PLZ8T-hyutVIEXMFgJ462Ou6Szjk26gPVo) of xCAD in action.

Visit [User Guide](https://xcad.net) page and start exploring the framework.

## Unit Tests

Solution contains unit and integration tests

To execute integration tests

* Download the [Test Data](https://1drv.ms/u/s!AjSRTGmPuUunpFTsZGgl4gfyjLRg?e=kZuO5c)
* Unzip into the folder
* Create an environment variable **XCAD_TEST_DATA** and set its value to the path of the folder above
* To test SOLIDWORKS Document Manager, add an environment variable **SW_DM_KEY** and set its value to your [Document Manager Key](https://www.codestack.net/solidworks-document-manager-api/getting-started/create-connection#activating-document-manager)
* Run tests
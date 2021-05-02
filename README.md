# ![Logo](data/icon.png) xCAD.NET

[![Base](https://img.shields.io/badge/-Base-yellow.svg)](https://xcad.xarial.com) [![NuGet version (xCAD.Base)](https://img.shields.io/nuget/v/Xarial.XCad.svg?style=flat-square)](https://www.nuget.org/packages/Xarial.XCad/)
[![Build status](https://dev.azure.com/xarial/xcad/_apis/build/status/base)](https://dev.azure.com/xarial/xcad/_build/latest?definitionId=13)---[![Toolkit](https://img.shields.io/badge/-Toolkit-purple.svg)](https://xcad.xarial.com) [![NuGet version (xCAD.Toolkit)](https://img.shields.io/nuget/v/Xarial.XCad.Toolkit.svg?style=flat-square)](https://www.nuget.org/packages/Xarial.XCad.Toolkit/)[![Build status](https://dev.azure.com/xarial/xcad/_apis/build/status/toolkit)](https://dev.azure.com/xarial/xcad/_build/latest?definitionId=14)---[![SOLIDWORKS](https://img.shields.io/badge/-SOLIDWORKS-red.svg)](https://xcad.xarial.com) [![NuGet version (xCAD.SolidWorks)](https://img.shields.io/nuget/v/Xarial.XCad.SolidWorks.svg?style=flat-square)](https://www.nuget.org/packages/Xarial.XCad.SolidWorks/)
[![Build status](https://dev.azure.com/xarial/xcad/_apis/build/status/solidworks)](https://dev.azure.com/xarial/xcad/_build/latest?definitionId=15)

## SOLIDWORKS API development made easy

[![User Guide](https://img.shields.io/badge/-Documentation-green.svg)](https://xcad.xarial.com)
[![Examples](https://img.shields.io/badge/-Examples-blue.svg)](https://github.com/xarial/xcad-examples)

[xCAD.NET](https://xcad.net) is a framework for building CAD agnostic applications. It allows developers to implement complex functionality with a very simple innovative approach. This brings the best user experience to the consumers of the software.

It has never been easier to create SOLIDWORKS add-ins with toolbar and menu commands.

~~~ cs
[ComVisible(true)]
public class MyAddIn : SwAddInEx
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

Framework reinvents the way you work with Property Manager Pages. No need to code a complex code behind for adding the controls and handling the values. Simply define your data model and the framework will build the suitable Property Manager Page automatically and two-way bind controls to the data model.

~~~ cs
public class MyPMPageData
{
    public string Text { get; set; }
    public int Number { get; set; }
    public ISwComponent Component { get; set; }
}

private ISwPropertyManagerPage<MyPMPageData> m_Page;
private MyPMPageData m_Data = new MyPMPageData();

private enum Commands_e
{
    ShowPmpPage
}

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
~~~

Complex macro features became ease with xCAD.NET

~~~ cs
[ComVisible(true)]
public class BoxMacroFeature : SwMacroFeatureDefinition
{
    public override CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument model, ISwMacroFeature feature)
    {
        var body = app.MemoryGeometryBuilder.CreateSolidBox(new Point(0, 0, 0),
            new Vector(1, 0, 0), new Vector(0, 1, 0),
            0.1, 0.1, 0.1);

        return new CustomFeatureBodyRebuildResult()
        {
            Bodies = body.Bodies.ToArray()
        };
    }
}
~~~

Watch the [video demonstration](https://www.youtube.com/watch?v=BuiFfv7-Qig) of xCAD in action.

Visit [User Guide](https://xcad.net) page and start exploring the framework.
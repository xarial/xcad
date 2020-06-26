---
title: Creating native property pages with xCAD framework
caption: Property Pages
description: Utilities for advanced development of SOLIDWORKS Property Manager Pages which enables data driven development with data binding
order: 3
---
Inspired by [PropertyGrid Control](https://msdn.microsoft.com/en-us/library/aa302326.aspx) in .NET Framework, xCAD brings the flexibility of data model driven User Interface into SOLIDWORKS API.

Framework allows to use data model structure as a driver of the User Interface. Framework will automatically generate required interface and implement the binding of the model.

This will greatly reduce the implementation time as well as make the property pages scalable, easily maintainable and extendable.

Property pages can be defined by data model and all controls will be automatically created and bound to the data.

~~~ cs jagged
[ComVisible(true)]
public class PmPage : SwPropertyManagerPageHandler 
{
    public SwSelObject Selection { get; set; }
    public SwEdge Edge { get; set; }
    public double NumberBox { get; set; }
    public string TextBox { get; set; }
}
~~~

~~~ cs jagged
private SwPropertyManagerPage<PmPage> m_Page;
private PmPage m_Data;
~~~

~~~ cs jagged
m_Page = CreatePage<PmPage>();
m_Page.Closed += OnPageClosed;
m_Data = new PmPage();
m_Page.Show(m_Data);
~~~

~~~ cs jagged
private void OnPageClosed(Xarial.XCad.UI.PropertyPage.Enums.PageCloseReasons_e reason)
{
    //TODO: handle
}
~~~
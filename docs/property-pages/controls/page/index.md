---
title: Options of SOLIDWORKS Property Manager Page
caption: Page
description: Overview of options applied to the SOLIDWORKS property manager page itself
image: property-manager-page.png
order: 2
---
![Property Manager Page style](property-manager-page.png)

1. Icon of the property manager page
1. Title of the property manager page
1. Links to documentation (what's new and help)
1. Control buttons (OK and Cancel)
1. Optional user message title
1. Optional user message content

Property manager page style can be customized by applying the **PageOptionsAttribute** onto the main class of the data model.

![Property page with OK and Cancel button options](pmpage-options.png)

<<< @/_src/PropertyPage/Controls/Page.cs#Options

Attributes allow to customize the buttons and behaviour of the page

## Attribution

![Property page with custom title, icon and message](pmpage-attributes.png)

Page title can be assigned via [DisplayNameAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.displaynameattribute?view=netframework-4.7.2)

Icon can be set via overloaded constructor of **PageOptionsAttribute**

Custom user message to provide additional information can be set via **MessageAttribute**

<<< @/_src/PropertyPage/Controls/Page.cs#Attribution

## Help Links

![Property page with help and what's new links](pmpage-help.png)

**HelpAttribute** allows providing links to help resources for your add-in. Framework will automatically open the specified url when user clicks corresponding help buttons in the property manager page:

<<< @/_src/PropertyPage/Controls/Page.cs#HelpLinks

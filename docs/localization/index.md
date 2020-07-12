---
title: Localizing SOLIDWORKS add-ins using xCAD.NET framework
caption: Localization
description: How to support multi language SOLIDWORKS add-ins by using of localized resources in xCAD framework
image: menu-localized.png
order: 7
---
xCAD frameworks supports [resources in .NET applications](https://docs.microsoft.com/en-us/dotnet/framework/resources/index) to enable localization of the add-in, e.g. supporting multiple languages.

This technique allows to load localized string at runtime based on the Windows settings in the control panel.

![Region and language page in Control Panel](region-format.png){ width=300 }

Resources should be added to the corresponding localized .resx file (e.g. Resources.resx for default, Resources.ru.resx for Russian, Resources.fr.resx for French, etc.)

![Resource files in the solutions](resource-files.png)

In order to reference the string from the resource, use the overloads of the constructors for the **TitleAttribute** and **SummaryAttribute** which allows to define title, tooltips, hint string for all elements across xCAD framework (i.e. menu commands, property page controls, macro feature, etc.)

Below is an example which demonstrates this technique. Text is localized as per resources below:

![Localized resource files in the Visual Studio](visual-studio-resources.png){ width=800 }

## Menu

Two commands in menu are localized for Russian and English versions of the add-in.

![Localized menu commands](menu-localized.png)

{% code-snippet { file-name: ~LocalizationAddIn.*, regions: [Commands] } %}

## Property Manager Page

Property Manager page title and tooltips for the controls are localized for Russian and English versions of the add-in.

![Localized Property Manager Page](property-page-localized.png)

{% code-snippet { file-name: ~LocalizationAddIn.*, regions: [PMPage] } %}

## Macro Feature

Macro feature base name is localized to Russian and English versions of the add-in.

> Note. Base name is only assigned while feature creation, feature won't be renamed after locale has changed.

![Localized Macro Feature base name](macro-feature-localized.png)

In a similar way it is possible to use strings from the resources to return another data, e.g. text of the error for macro feature.

![Localized macro feature error](macro-feature-error-localized.png)

{% code-snippet { file-name: ~LocalizationAddIn.*, regions: [MacroFeature] } %}

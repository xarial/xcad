---
title: Defining commands buttons in SOLIDWORKS toolbar using xCAD framework
caption: Defining Commands
description: Explanations on the ways of defining the commands in groups using xCAD framework for SOLIDWORKS add-ins in C# and VB.NET
order: 1
---
## Defining Commands

xCAD framework allows defining the commands in the enumeration (enum). In this case the enumeration value become the id of the corresponding command.

{% code-snippet { file-name: ~Extension\CommandsManager\DefiningCommands.* } %}

## Commands Decoration

Commands can be decorated with the additional attributes to define look and feel of the command.

### Title

User friendly title can be defined using the **TitleAttribute**. Alternatively, any attribute class which inherits [DisplayNameAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.displaynameattribute?view=netframework-4.0) is supported as a title.

### Description

Description is a text displayed in the SOLIDWORKS command bar when user hovers the mouse over the command. Description can be defined using the [DescriptionAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.descriptionattribute?view=netframework-4.0)

### Icon

Icon can be set using the **IconAttribute**.

xCAD framework will scale the icon appropriately to match the version of SOLIDWORKS. For example for SOLIDWORKS 2016 onwards, 6 icons will be created to support high resolution, for older SOLIDWORKS, 2 icons will be created (large and small).

Transparency is supported. xCAD framework will automatically assign the required transparency key for compatibility with SOLIDWORKS.

Icons can be referenced from any static class. Usually this should be a resource class. It is required to specify the type of the resource class as first parameter, and the resource names as additional parameters. Use *nameof* keyword to load the resource name to avoid usage of 'magic' strings.

{% code-snippet { file-name: ~Extension\CommandsManager\CommandsAttribution.* } %}

## Commands Scope

Each command can be assigned with the operation scope (i.e. the environment where this command can be executed, e.g. Part, Assembly etc.). Scope can be assigned with **CommandItemInfoAttribute** attribute by specifying the values in *suppWorkspaces* parameter of the attribute's constructor. The **WorkspaceTypes_e** is a flag enumeration, so it is possible to combine the workspaces.

Framework will automatically disable/enable the commands based on the active environment as per the specified scope. For additional logic for assigning the state visit [Custom Enable Command State](/extension/commands/command-states/) article.

{% code-snippet { file-name: ~Extension\CommandsManager\CommandsScope.* } %}

## User Assigned Command Group IDs

**CommandGroupInfoAttribute** allows to assign the static command id to the group. This should be applied to the enumerator definition. If this attribute is not used SwEx framework will assign the ids automatically.

{% code-snippet { file-name: ~Extension\CommandsManager\CommandGroupId.* } %}


---
title: xCAD Framework enables easy and robust development of add-ins with SOLIDWORKS API
caption: Extensions
description: Advanced utilities for the development of powerful SOLIDWORKS add-ins using SOLIDWORKS API in .NET (C# and VB.NET). Framework simplifies the creation and maintaining of commands and UI elements.
order: 2
---
xCAD provides utilities for simplified development of SOLIDWORKS add-ins. These types of applications running in-process and enables best user experience.

The functionality includes

* Automatic registration of the add-in
* Simplified commands groups management
* Events management
* Task Panes, Feature Manager Tab, Model View Tab

<div class="video_wrapper">
    <iframe frameborder="0" width="100%" height="100%" src="https://www.youtube.com/embed/IyUkJf7xmLY" allowfullscreen=""></iframe>
</div>

## Features Overview

Although some of the feature below, such as documents and events management, reading or writing to the 3rd party streams can be used from [Stand-Alone applications](/stand-alone/), in most cases this functionality is used within add-ins.

### Registering Add-In

To Register add-in just declare a public class and add COMVisible attribute (no need to run custom regasm commands, no need to call any static classes).

<<< @/_src/Extension/Overview.cs#Register

### Adding Commands

Commands can be defined by creating an enumerations. Commands can be customized by adding attributes to assign title, tooltip, icon etc. Commands can be grouped under sub menus. Simply specify the image (transparency is supported) and framework will create required bitmaps compatible with SOLIDWORKS. No need to assign gray background to enable transparency, no need to scale images to fit the required sizes - simply use any image and framework will do the rest. Use resources to localize the add-in.

<<< @/_src/Extension/Overview.cs#CommandGroup
<<< @/_src/Extension/Overview.cs#CommandGroup2
<<< @/_src/Extension/Overview.cs#CommandGroup3

### Managing Documents Lifecycle and Events

Framework will manage the lifecycle of documents by wrapping them in the specified class and allows to handle common events:

<<< @/_src/Extension/Overview.cs#DocHandler

### Reading and Writing to 3rd Party Storage and Store

It has never been easier to read and write data to the internal SOLIDWORKS file storage. Simply override the corresponding event and serialize/deserialize the data using XML, DataContract, Binary etc. serializers:

<<< @/_src/Extension/Overview.cs#3rdParty

### Hosting User Controls In SOLIDWORKS Panels

Just specify User Control to host and framework will do the rest:

#### Task Pane

<<< @/_src/Extension/Overview.cs#TaskPane
<<< @/_src/Extension/Overview.cs#TaskPane2
<<< @/_src/Extension/Overview.cs#TaskPane3
<<< @/_src/Extension/Overview.cs#TaskPane4

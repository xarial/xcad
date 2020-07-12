---
title: xCAD.NET framework for developing applications for CAD systems
caption: xCAD.NET
description: Framework for .NET (C# and VB.NET) to create modern application for CAD systems (SOLIDWORKS)
image: logo.svg
---
![xCAD.NET framework](logo.svg){ width=150 }

xCAD.NET is a framework which is designed to simplify development of software for SOLIDWORKS in .NET (C# and VB.NET).

Framework provides utilities for implementation software design principles such as [S.O.L.I.D](https://en.wikipedia.org/wiki/SOLID), [type safety](https://en.wikipedia.org/wiki/Type_safety), **single point of maintenance** for developing maintainable and scalable solutions for SOLIDWORKS and other CAD systems.

There are 3 main section of CAD functionality which are covered by framework

* [Extensions](extensions) - add-in skeleton, commands, menus, toolbars, events management, data access
* [Property Pages](property-pages) - building native property pages with data binding
* [Custom Feature](custom-features) - building parametric native features

Framework enables the abstraction layers over the CAD API allowing CAD agnostic development.

* Interfaces defined in the [Xarial.XCad](https://www.nuget.org/packages/Xarial.XCad/) provide highest level of abstraction and completely hide the references to any CAD system, neither reference any interops or namespaces. Use this to develop CAD agnostic applications. All interface names start with *IX*, e.g. IXApplication, IXDocument, IXFace
* Interfaces defined in [Xarial.XCad.SolidWorks](https://www.nuget.org/packages/Xarial.XCad.SolidWorks/) or other CAD systems (future). This is an implementation of the base interfaces. This library contains references to specific CAD system and might contain functionality specific to this CAD system. For example *SwApplication* is an implementation of *IXApplication*, *SwDocument* is an implementation of *IXDocument*. Naming convention follows the short name of the CAD system at the start of the name.
* Access to native APIs. All the xCAD wrapper classes provide access to native (underlying) APIs. For example **SwApplication.Sw** would return the pointer to [ISldWorks](http://help.solidworks.com/2012/english/api/sldworksapi/solidworks.interop.sldworks~solidworks.interop.sldworks.isldworks.html), **SwDocument.Model** returns the [IModelDoc2](http://help.solidworks.com/2012/english/api/sldworksapi/solidworks.interop.sldworks~solidworks.interop.sldworks.imodeldoc2.html) and **SwEntity.Entity** points to [IEntity](http://help.solidworks.com/2012/english/api/sldworksapi/solidworks.interop.sldworks~solidworks.interop.sldworks.ientity.html)

Framework source code is available on [GitHub](https://github.com/xarial/xcad) under [MIT](license) license;
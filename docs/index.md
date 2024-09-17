---
layout: home

title: xCAD.NET framework for developing applications for CAD systems
titleTemplate: xCAD.NET is a framework which is designed to simplify development of software

hero:
  name: xCAD.NET
  text: Designed to simplify development of software for SOLIDWORKS 
  tagline: .NET (C# and VB.NET)
  actions:
    - theme: brand
      text: Getting started
      link: /installation/
    - theme: alt
      text: Watch video!
      link: https://youtu.be/BuiFfv7-Qig
    - theme: alt
      text: Examples
      link: https://github.com/xarial/xcad-examples
    - theme: alt
      text: GitHub
      link: https://github.com/xarial/xcad
    - theme: alt
      text: MIT
      link: license
 
  image:
    src: /logo.svg
    alt: xCAD.NET

features:
  - icon: 🧩
    title: Extensions
    details: Add-in skeleton, commands, menus, toolbars, events management, data access.
    link: extensions
  - icon: 💻
    title: Property Pages
    details: Building native property pages with data binding.
    link: property-pages
  - icon: 🏗
    title: Custom Feature
    details: Building parametric native features.
    link: custom-features

---

Framework provides utilities for implementation software design principles such as [S.O.L.I.D](https://en.wikipedia.org/wiki/SOLID), [type safety](https://en.wikipedia.org/wiki/Type_safety), **single point of maintenance** for developing maintainable and scalable solutions for SOLIDWORKS and other CAD systems.

## Architecture

![xCAD.NET architecture diagram](diagram.svg){ width=800 }

Framework enables the abstraction layers over the CAD API allowing CAD agnostic development.

* Interfaces defined in the [Xarial.XCad](https://www.nuget.org/packages/Xarial.XCad/) provide highest level of abstraction and completely hide the references to any CAD system, neither reference any interops or namespaces. Use this to develop CAD agnostic applications. All interface names start with *IX*, e.g. IXApplication, IXDocument, IXFace
* Interfaces defined in [Xarial.XCad.SolidWorks](https://www.nuget.org/packages/Xarial.XCad.SolidWorks/), [Xarial.XCad.SwDocumentManager](https://www.nuget.org/packages/Xarial.XCad.SwDocumentManager/), [Xarial.XCad.Inventor](https://www.nuget.org/packages/Xarial.XCad.Inventor/) or other CAD systems (future). This is an implementation of the base interfaces. This library contains references to specific CAD system and might contain functionality specific to this CAD system. For example *ISwApplication* is an implementation of *IXApplication* and *ISwDocument* is an implementation of *IXDocument* in SOLIDWORKS, while *IAiApplication* and *IAiDocument* are corresponding implementations in Autodesk Inventor. Naming convention follows the short name of the CAD system at the start of the name.
* Access to native APIs. All the xCAD wrapper classes provide access to native (underlying) APIs. For example **ISwApplication.Sw** would return the pointer to [ISldWorks](http://help.solidworks.com/2012/english/api/sldworksapi/solidworks.interop.sldworks~solidworks.interop.sldworks.isldworks.html), **ISwDocument.Model** returns the [IModelDoc2](http://help.solidworks.com/2012/english/api/sldworksapi/solidworks.interop.sldworks~solidworks.interop.sldworks.imodeldoc2.html) and **ISwEntity.Entity** points to [IEntity](http://help.solidworks.com/2012/english/api/sldworksapi/solidworks.interop.sldworks~solidworks.interop.sldworks.ientity.html)
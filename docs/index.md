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
  - icon: 🤝
    title: Hight level interface abstractions
    details: |
      Use this to develop CAD agnostic applications. All interface names start with *IX*, e.g. <br> <br> 
      IXApplication,<br> 
      IXDocument,<br> 
      IXFace
    link: https://www.nuget.org/packages/Xarial.XCad
  - icon: 🤝
    title: Custom implementation of CAD interfaces.
    details: |
      Contain functionality specific to this CAD system. <br>
        ISwApplication => IXApplication <br>
        ISwDocument =>  IXDocument <br>
        IAiApplication =>  IAiApplication
  - icon: 🛠️
    title: Access to native APIs
    details: |
      All the xCAD wrapper classes provide access to native (underlying) APIs. <br> <br> 
        ISwApplication.Sw => ISldWorks <br>
        ISwDocument.Model => IModelDoc2<br> 
        ISwEntity.Entity => IEntity
---

Framework provides utilities for implementation software design principles such as [S.O.L.I.D](https://en.wikipedia.org/wiki/SOLID), [type safety](https://en.wikipedia.org/wiki/Type_safety), **single point of maintenance** for developing maintainable and scalable solutions for SOLIDWORKS and other CAD systems.

## Architecture

![xCAD.NET architecture diagram](diagram.svg){ width=800, style="margin: 0 auto" }

# xCAD.NET Templates

## SOLIDWORKS Add-In [C#, VB.NET]

This templates generates a project scaffolding for creating basic add-in in SOLIDWORKS with multiple options.

### Add Command Manager

Adds the sample command manager with buttons which shows the About dialog and create sample cube geometry

### Add Property Page

Adds property manager page to collect user input for the parameters of the cube and creates cube geometry upon closing of property manager page

### Add Macro Feature

Shows an examples of creating parametric cube feature with dimensions and property manager page editor using macro feature

## SOLIDWORKS Macro Feature [C#]

This templates generates a project scaffolding for creating basic macro feature element in SOLIDWORKS. Macro feature will generate a parametric cylinder geometry with multiple options.

### Add Editor

Adds an editor of the macro feature parameter via property manager page

### Add Dimensions

Adds dimensions to the macro feature which can be edited directly from the graphics view.

### Supports In-Context Editing

Enables insertion and editing of the cylinder macro feature from the context of the assembly allowing the external relations.

### Supports Edit Bodies

Demonstrates how to edit existing bodies and append or subtract the cylindrical geometry

## Cross-CAD Console Application [C#]

Adds sample console application to read properties from files, configurations and cut-list items. User needs to provide the path to the files as arguments. Business logic is implemented using the CAD-agnostic approach using base interfaces of xCAD.

### SOLIDWORKS

Provides an implementation for SOLIDWORKS

### SOLIDWORKS Document Manager

Provides an implementation for SOLIDWORKS Document Manager API

### Inventor

Provides an implementation for Autodesk Inventor

## Cross-CAD WPF Application [C#]

Adds sample WPF application to read properties from files, configurations and cut-list items and display in the data grid. User can select the version of the application and the file to read properties from. Business logic is implemented using the CAD-agnostic approach using base interfaces of xCAD.

### SOLIDWORKS

Provides an implementation for SOLIDWORKS

### SOLIDWORKS Document Manager

Provides an implementation for SOLIDWORKS Document Manager API

### Inventor

Provides an implementation for Autodesk Inventor
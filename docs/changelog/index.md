---
title: List of changes in the releases of xCAD.NET framework
caption: Changelog
description: Information about releases (new features, bug fixes breaking changes) of xCAD.NET framework for developing CAD applications
order: 8
---
This page contains list of the most notable changes in the releases of xCAD.NET.

Breaking change is marked with &#x26A0; symbol

## 0.6.3 - September, 30 2020

* Added exceptions for the macro running and document opening
* &#x26A0; Changed SwApplication::Start to be sync
* Implemented [#29 - IXDocumentRepository::Open should support all file types](https://github.com/xarial/xcad/issues/29) 

## 0.6.2 - September, 28 2020

* Fixed [#24 - Build error when cleaning the solution](https://github.com/xarial/xcad/issues/24)
* Implemented [#25 - Add IXApplication::Process](https://github.com/xarial/xcad/issues/25)

## 0.6.1 - September, 23 2020

* Fixed [#20 - BitmapButton bool not firing propertyManagerPage DataChanged Event](https://github.com/xarial/xcad/issues/20)
* Implemented [#21 - Add IXApplication::WindowHandle](https://github.com/xarial/xcad/issues/21)
* Implemented [#22 - Add SwApplication::GetInstalledVersion static method](https://github.com/xarial/xcad/issues/22)

## 0.6.0 - September, 13 2020

* Implemented [#5 - Updating Combobox based on another comboBox selection change](https://github.com/xarial/xcad/issues/5). Refer [help documentation](/property-pages/controls/combo-box#dynamic-items-provider) for more information

* Implemented [#6 - Add Support to Bitmap Button](https://github.com/xarial/xcad/issues/6)

* &#x26A0; Moved **Xarial.XCad.Utils.PageBuilder.Base.IDependencyHandler** to **Xarial.XCad.UI.PropertyPage.Services.IDependencyHandler**

* &#x26A0; Added second parameter **IControl[] dependencies** to **ICustomItemsProvider.ProvideItems** 

* &#x26A0; **IDependencyHandler.UpdateState** provides **IControl** instead of **IBinding**

## 0.5.8 - September, 1 2020

* Added new events:
    
    * IXConfigurationRepository.ConfigurationActivated
    * IXDocument.Rebuild, IXDocument.Saving
    * IXDocumentCollection.DocumentActivated
    * IXSheetRepository.SheetActivated

* Added new interfaces
    * IXSheet

* &#x26A0; Added parameter of **IXDocument** to **NewSelectionDelegate**

* Fixed the issue with toolbar positions not maintained after SOLIDWORKS restart

* &#x26A0; **state** parameter of **CommandStateDelegate** is no longer passed with **ref** keyword

## 0.5.7 - July, 19 2020

* Added support for TaskPane
* Added support for Feature Manager Tab

## 0.5.0 - June, 15 2020

* Added support for tabs and custom controls in property pages
* Added support for 3rd party storage and 3rd party stream
* Renamed to **StandardIconAttribute** to **StandardControlIconAttribute**

## 0.3.1 - February, 9 2020

* &#x26A0; Renamed **ControlAttributionAttribute** to **StandardIconAttribute**

## 0.2.4 - February, 6 2020

* Added **ICustomItemsProvider** to provide dynamic items for the ComboBox control in property pages

## 0.2.0 - February, 6 2020

* Added support for selections
* Added support for IXFace

## 0.1.0 - February, 4 2020

Initial Release
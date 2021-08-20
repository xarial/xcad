---
title: List of changes in the releases of xCAD.NET framework
caption: Changelog
description: Information about releases (new features, bug fixes breaking changes) of xCAD.NET framework for developing CAD applications
order: 99
---
This page contains list of the most notable changes in the releases of xCAD.NET.

Breaking change is marked with &#x26A0; symbol

## 0.7.5

* &#x26A0; ISwMacroFeature::ToParameters is removed. Now SwObjectFactory::FromDispatch will create a specific instance from IFeature
* &#x26A0; ISwBodyExtension::ToTempBody is replace with IXBody::Copy
* &#x26A0; IXPlanarSheet::Boundary type is changed from IXSegment[] to IXRegion. Use IXGeometryBuilder::CreateRegionFromSegments to create region from array of segments
* &#x26A0; IXPlanarSheet::Boundary is renamed to IXPlanarSheet::Region
* &#x26A0; IXDrawingView::Document is renamed to IXDrawingView::ReferencedDocument
* &#x26A0; IXComponent::Document is renamed to IXComponent::ReferencedDocument
* &#x26A0; SwObjectFactory::FromDispatch is replaced with ISwDocument::CreateObjectFromDispatch and ISwApplication::CreateObjectFromDispatch
* &#x26A0; Changed the signatures of SwMacroFeatureDefinition{TParams, TPage}::OnEditingCompleted, SwMacroFeatureDefinition{TParams, TPage}::OnFeatureInserted
* &#x26A0; Changed from protected to public: SwMacroFeatureDefinition{TParams, TPage}::OnEditingStarted, SwMacroFeatureDefinition{TParams,TPage}::CreatePageHandler, SwMacroFeatureDefinition{TParams,TPage}::OnEditingStarted, SwMacroFeatureDefinition{TParams,TPage}::OnEditingCompleted, SwMacroFeatureDefinition{TParams,TPage}::OnFeatureInserted, SwMacroFeatureDefinition{TParams,TPage}::CreateDynamicControls
* &#x26A0; IXGeometryMemoryBuilder::PreCreateArc is renamed to IXGeometryMemoryBuilder::PreCreateCircle. IXGeometryMemoryBuilder::PreCreateArc has been redefined to create arc rather than circle

## 0.7.4 - July 11, 2021

* Fixed the incorrect mass properties for SOLIDWORKS 2019 and older
* Fixed invalid principle moment of inertia and principle axis of inertia calculation for Part file

## 0.7.3 - July 2, 2021

* &#x26A0; IXDocument::DeserializeObject changed to use generic parameter to specify return type
* Fixed the IXDocumentRepository::Active error
* Fixed the issue with macro feature editor not using the converted parameters correctly if different from the page data
* Added suppressed icon for the macro feature
* [Dispose objects in IXExtension safely one-by-one](https://github.com/xarial/xcad/issues/72)
* [Add option to specify the order of controls in Property Page](https://github.com/xarial/xcad/issues/73)
* [Add support for Mass properties](https://github.com/xarial/xcad/issues/74)
* &#x26A0; [Add option to calculate bounding box relative to coordinate system](https://github.com/xarial/xcad/issues/75)

## 0.7.1 - June 8, 2021

* &#x26A0; IXDocument3D::CalculateBoundingBox is replaced with IXDocument3D::PreCreateBoundingBox
* &#x26A0; ComponentState_e::Rapid is renamed to ComponentState_e::Lightweight
* Implemented [Add option to calculate bounding box relative to coordinate system](https://github.com/xarial/xcad/issues/75)
* Implemented [Add support for Mass properties](https://github.com/xarial/xcad/issues/74)

## 0.7.0 - May 2, 2021

* &#x26A0; IXPropertyRepository::GetOrPreCreate moved to extension method
* &#x26A0; IXObject::IsSame replaced with IEquatable<IXObject>.Equals
* &#x26A0; IXCustomControl::DataContextChanged replaced with IXCustomControl::ValueChanged
* &#x26A0; IXCustomControl::DataContext replaced with IXCustomControl::Value
* &#x26A0; ResourceHelper::FromBytes replaced with BaseImage class
* &#x26A0; CustomItemsAttribute is renamed to ComboBoxAttribute
* Implemented [Implement xCAD for SOLIDWORKS Document Manager](https://github.com/xarial/xcad/issues/17)
* Implemented [Add support for cut-list custom properties enhancement](https://github.com/xarial/xcad/issues/18)
* Implemented [Add support for List control in the property page enhancement](https://github.com/xarial/xcad/issues/27)
* Implemented [Add support for checkbox in group enhancement](https://github.com/xarial/xcad/issues/54)
* Implemented [Add attribute to exclude the property from the macro feature binding enhancement](https://github.com/xarial/xcad/issues/61)
* Implemented [Add ability to serialize and deserialize pointers for SW objects enhancement](https://github.com/xarial/xcad/issues/62)
* Implemented [Add ability to add controls dynamically to property manager page enhancement](https://github.com/xarial/xcad/issues/63)
* Implemented [Add options to specify items source based on the property for ComboBox control enhancement](https://github.com/xarial/xcad/issues/64)
* Implemented [Add support for expressions for custom properties enhancement](https://github.com/xarial/xcad/issues/65)
* Implemented [Add support for quantity for the configuration enhancement](https://github.com/xarial/xcad/issues/66)
* Fixed [API returns named view from the sheet (when not inserted) bug](https://github.com/xarial/xcad/issues/67)
* Fixed [Bitmap button displays incorrect size on first opening the page bug](https://github.com/xarial/xcad/issues/68)
* &#x26A0; Implemented [Components should be returned from IXConfiguration not IXAssembly enhancement](https://github.com/xarial/xcad/issues/69)
* &#x26A0; Fixed [Selection color is ignored in SelectionBoxOption attribute bug](https://github.com/xarial/xcad/issues/70)

## 0.6.10 - December 7, 2020

* &#x26A0; IXComponent::IsResolved replaced with IXComponent::State
* &#x26A0; ISwApplication::Version is changed from SwVersion_e to SwVersion class
* &#x26A0; SwApplicationFactory::GetInstalledVersions returns IEnumerable of ISwVersion instead of IEnumerable of SwVersion_e
* Implemented [#55 - Add option to extract all dependencies from the document](https://github.com/xarial/xcad/issues/55)
* Implemented [#56 - Add APIs to save document](https://github.com/xarial/xcad/issues/56)
* Implemented [#57 - Add support for version on IXDocument and IXApplication enhancement](https://github.com/xarial/xcad/issues/57)
* Fixed [#58 - Document events are not attached for pre-created templates](https://github.com/xarial/xcad/issues/58)
* Fixed [#58 - Error when opening documents breaks IXDocumentRepository](https://github.com/xarial/xcad/issues/59)

## 0.6.9 - November 27, 2020

* &#x26A0; IXDocument::Visible, IXDocument::ReadOnly, IXDocument::ViewOnly, IXDocument::Rapid, IXDocument::Silent are replaced with IXDocument::State
* &#x26A0; IXServiceConsumer::ConfigureServices renamed to IXServiceConsumer::OnConfigureServices 
* Implemented [#46 - Add IXComponent::Path](https://github.com/xarial/xcad/issues/46)
* Fixed [#47 - Custom controls added to Property Page are not loaded after page is closed](https://github.com/xarial/xcad/issues/47)
* Implemented [#48 - Add support for progress bar in application](https://github.com/xarial/xcad/issues/48)
* Implemented [#49 - Allow to specify template when creating new document](https://github.com/xarial/xcad/issues/49)
* Fixed [#50 - Document management breaks if custom doc handler has an unhandled exception](https://github.com/xarial/xcad/issues/50)
* Fixed [#51 - IXAssembly::Components empty for the LDR assembly](https://github.com/xarial/xcad/issues/51)

## 0.6.8 - November 10, 2020

* Added tags support for IXDocument to store custom user data within the session
* Added the IXPart::CutListRebuild event

## 0.6.7 - November 9, 2020

* &#x26A0; All SOLIDWORKS specific classes replaced with corresponding interfaces with I at the start (e.g. SwApplication -> ISwApplication, SwDocument -> ISwDocument)
* &#x26A0; IXDocumentRepository::Open is replaced with transaction (also available as extension method) and **DocumentOpenArgs** is retired.
* &#x26A0; IXModelViewBasedDrawingView::View is renamed to IXModelViewBasedDrawingView::SourceModelView
* &#x26A0; IXCircularEdge::Center, IXCircularEdge::Axis, IXCircularEdge::Radius are replaced with IXCircularEdge::Definition
* &#x26A0; IXLinearEdge::RootPoint, IXLinearEdge::Direction are replaced with IXLinearEdge::Definition
* &#x26A0; IXGeometryBuilder is changed and available via IXApplication::MemoryGeometryBuilder
* Added support for extrusion, sweep, revolve for memory IXGeometryBuilder
* Added partial support for surfaces and curves as definitions for edges and faces
* Added partial support for sketch entities in the sketch

## 0.6.6 - October 29, 2020

* Implemented [#36 - Add ability to configure services for dependency injection](https://github.com/xarial/xcad/issues/36)
* Implemented [#37 - Add options to add colors to faces, bodies and features](https://github.com/xarial/xcad/issues/37)
* Implemented [#38 - Add support for drawing views](https://github.com/xarial/xcad/issues/38)
* Implemented [#39 - Add ability to read feature tree from IXComponent](https://github.com/xarial/xcad/issues/39)
* Fixed [#40 - SwAssembly.Components returns empty enumerable in add-in bug](https://github.com/xarial/xcad/issues/40)
* Fixed [#41 - IXSelectionRepository::Add fails if other objects were preselected bug](https://github.com/xarial/xcad/issues/41)
* &#x26A0; IXProperty::Exists moved to an extension method instead of property
* &#x26A0; IXDocument3D::ActiveView moved to IXDocument3D::Views::Active
* &#x26A0; IXDocumentCollection renamed to IXDocumentRepository

## 0.6.5 - October 14, 2020

* Implemented [#33 - Add event when extension and host application is fully loaded](https://github.com/xarial/xcad/issues/33)
* Implemented [#34 - Add WindowRectangle API to find the bounds of the host window](https://github.com/xarial/xcad/issues/34)

## 0.6.4 - September 30, 2020

* Implemented [#30 - Add option to open document in rapid mode](https://github.com/xarial/xcad/issues/30)
* Fixed [#31 - INotifyPropertyChanged is ignored](https://github.com/xarial/xcad/issues/31)
* Switched SOLIDWORKS Interops to version 2020

## 0.6.3 - September 30, 2020

* Added exceptions for the macro running and document opening
* &#x26A0; Changed SwApplication::Start to be sync
* Implemented [#29 - IXDocumentRepository::Open should support all file types](https://github.com/xarial/xcad/issues/29) 

## 0.6.2 - September 28, 2020

* Fixed [#24 - Build error when cleaning the solution](https://github.com/xarial/xcad/issues/24)
* Implemented [#25 - Add IXApplication::Process](https://github.com/xarial/xcad/issues/25)

## 0.6.1 - September 23, 2020

* Fixed [#20 - BitmapButton bool not firing propertyManagerPage DataChanged Event](https://github.com/xarial/xcad/issues/20)
* Implemented [#21 - Add IXApplication::WindowHandle](https://github.com/xarial/xcad/issues/21)
* Implemented [#22 - Add SwApplication::GetInstalledVersion static method](https://github.com/xarial/xcad/issues/22)

## 0.6.0 - September 13, 2020

* Implemented [#5 - Updating Combobox based on another comboBox selection change](https://github.com/xarial/xcad/issues/5). Refer [help documentation](/property-pages/controls/combo-box#dynamic-items-provider) for more information

* Implemented [#6 - Add Support to Bitmap Button](https://github.com/xarial/xcad/issues/6)

* &#x26A0; Moved **Xarial.XCad.Utils.PageBuilder.Base.IDependencyHandler** to **Xarial.XCad.UI.PropertyPage.Services.IDependencyHandler**

* &#x26A0; Added second parameter **IControl[] dependencies** to **ICustomItemsProvider.ProvideItems** 

* &#x26A0; **IDependencyHandler.UpdateState** provides **IControl** instead of **IBinding**

## 0.5.8 - September 1, 2020

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

## 0.5.7 - July 19, 2020

* Added support for TaskPane
* Added support for Feature Manager Tab

## 0.5.0 - June 15, 2020

* Added support for tabs and custom controls in property pages
* Added support for 3rd party storage and 3rd party stream
* Renamed to **StandardIconAttribute** to **StandardControlIconAttribute**

## 0.3.1 - February 9, 2020

* &#x26A0; Renamed **ControlAttributionAttribute** to **StandardIconAttribute**

## 0.2.4 - February 6, 2020

* Added **ICustomItemsProvider** to provide dynamic items for the ComboBox control in property pages

## 0.2.0 - February 6, 2020

* Added support for selections
* Added support for IXFace

## 0.1.0 - February 4, 2020

Initial Release
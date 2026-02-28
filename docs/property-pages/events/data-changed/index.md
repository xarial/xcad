---
title: SOLIDWORKS Property Manager Page data changed events handling
caption: Data Change
description: Overview of events associated with data change of SOLIDWORKS property manager page handled in xCAD framework
order: 2
---
xCAD framework provides event handlers for the data changes in the controls. Use this handlers to update preview or any other state which depends on the values in the controls.

## Post data changed event

**Xarial.XCad.SolidWorks.UI.PropertyPage.ISwPropertyManagerPage`<TModel>`.DataChanged** event is raised after the user changed the value in the control which has updated the data model. Refer the bound data model for new values.

<<< @/_src/PropertyPage/Events.cs#DataChanged
<<< @/_src/PropertyPage/Events.cs#DataChanged2

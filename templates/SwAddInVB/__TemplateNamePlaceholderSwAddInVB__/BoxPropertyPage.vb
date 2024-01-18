Imports __TemplateNamePlaceholderSwAddInVB__.Sw.My.Resources
Imports System
Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports Xarial.XCad
Imports Xarial.XCad.Base.Attributes
Imports Xarial.XCad.Features
Imports Xarial.XCad.Geometry
Imports Xarial.XCad.SolidWorks.UI.PropertyPage
Imports Xarial.XCad.UI.PropertyPage.Attributes
Imports Xarial.XCad.UI.PropertyPage.Base
Imports Xarial.XCad.UI.PropertyPage.Enums
Imports Xarial.XCad.UI.PropertyPage.Services
Imports Xarial.XCad.UI.PropertyPage.Structures

Namespace __TemplateNamePlaceholderSwAddInVB__.Sw
    <ComVisible(True)>
    <Guid("938981F5-32AB-4971-B799-1DEEAEBE6188")>
    <Icon(GetType(Resources), NameOf(Resources.box_icon))>
    <Title("Create Box")>
    Public Class BoxPropertyPage
        Inherits SwPropertyManagerPageHandler
        Private Class PlanarRegionSelectionFilter
            Implements ISelectionCustomFilter
            Public Sub Filter(selBox As IControl, selection As IXSelObject, args As SelectionCustomFilterArguments) Implements ISelectionCustomFilter.Filter
                'only allow planar region (e.g. planar faces or planes)
                args.Filter = TypeOf selection Is IXPlanarRegion

                If Not args.Filter Then
                    'show the custom warning to the user of why this entity cannot be selected
                    args.Reason = "Select planar face or plane"
                End If
            End Sub
        End Class

        Public Class LocationGroup
            'any selectable entity will be rendered as the selection box
            <StandardControlIcon(BitmapLabelType_e.SelectFace)>
            <Description("Face or plane to place box on")>
            <SelectionBoxOptions(Filters:=New Type() {GetType(IXFace), GetType(IXPlane)}, CustomFilter:=GetType(PlanarRegionSelectionFilter))>
            Public Property PlaneOrFace As IXEntity 'default filter will only allow selection of faces and planes and custom filter will additionlly excluded non planar faces
        End Class

        Public Class ParametersGroup
            'public property of type double will be rendered as the number box
            <NumberBoxOptions(NumberBoxUnitType_e.Length, 0, 1000, 0.01, False, 0.02, 0.001)>
            <Description("Width of the box")>
            <Icon(GetType(Resources), NameOf(Resources.width_icon))>
            Public Property Width As Double = 0.1

            <NumberBoxOptions(NumberBoxUnitType_e.Length, 0, 1000, 0.01, False, 0.02, 0.001)>
            <Description("Height of the box")>
            <Icon(GetType(Resources), NameOf(Resources.height_icon))>
            Public Property Height As Double = 0.1

            <NumberBoxOptions(NumberBoxUnitType_e.Length, 0, 1000, 0.01, False, 0.02, 0.001)>
            <Description("Length of the box")>
            <Icon(GetType(Resources), NameOf(Resources.length_icon))>
            Public Property Length As Double = 0.1
        End Class

        'classes will be rendered as property manager page groups
        Public ReadOnly Property Location As LocationGroup
        Public ReadOnly Property Parameters As ParametersGroup

        Public Sub New()
            Location = New LocationGroup()
            Parameters = New ParametersGroup()
        End Sub
    End Class
End Namespace

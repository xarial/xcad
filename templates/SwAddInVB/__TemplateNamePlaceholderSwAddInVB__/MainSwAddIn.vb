#If (_AddCommandManager_ Or _AddPropertyPage_ Or _AddMacroFeature_) Then
Imports __TemplateNamePlaceholderSwAddInVB__.Sw.My.Resources
#End If
#If (_AddCommandManager_ Or _AddPropertyPage_) Then
Imports SolidWorks.Interop.swconst
#End If
Imports System
Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports Xarial.XCad.Base
Imports Xarial.XCad.Base.Attributes
Imports Xarial.XCad.Base.Enums
Imports Xarial.XCad.Features
Imports Xarial.XCad.Geometry
Imports Xarial.XCad.Geometry.Structures
Imports Xarial.XCad.SolidWorks
Imports Xarial.XCad.UI.Commands
Imports Xarial.XCad.UI.Commands.Attributes
Imports Xarial.XCad.UI.Commands.Enums
Imports Xarial.XCad.UI.PropertyPage
Imports Xarial.XCad.UI.PropertyPage.Enums
Imports Xarial.XCad.UI.PropertyPage.Structures

Namespace __TemplateNamePlaceholderSwAddInVB__.Sw
    <ComVisible(True)>
    <Guid("C472AA4E-58F6-4D7B-8458-834033016A77")>
    <Title("__TemplateNamePlaceholderSwAddInVB__ SOLIDWORKS Add-In")>
    <Description("SOLIDWORKS add-in created with xCAD.NET")>
    Public Class MainSwAddIn
        Inherits SwAddInEx
#If (_AddCommandManager_ Or _AddPropertyPage_ Or _AddMacroFeature_) Then
        'command groups can be created by defining the enumeration and all its fields will be rendered as buttons
        <Title("__TemplateNamePlaceholderSwAddInVB__")>
        <Description("Commands of __TemplateNamePlaceholderSwAddInVB__")>
        Private Enum Commands_e
#If (_AddCommandManager_ Or _AddPropertyPage_)
            <Icon(GetType(Resources), NameOf(Resources.box_icon))>
            <Title("Create Box")>
            <Description("Creates box using standard feature")>
            <CommandItemInfo(True, True, WorkspaceTypes_e.Part Or WorkspaceTypes_e.InContextPart, True)>
            CreateBox

#End If
#If _AddMacroFeature_
            <Icon(GetType(Resources), NameOf(Resources.parametric_box_icon))>
            <Title("Create Parametric Box")>
            <Description("Creates parametric macro feature")>
            <CommandItemInfo(True, True, WorkspaceTypes_e.Part, True)>
            CreateParametricBox

#End If
            'button can have custom attribute to define its look and feel
            <Icon(GetType(Resources), NameOf(Resources.about_icon))>
            <Title("About...")>
            <Description("Shows About Box")>
            <CommandItemInfo(True, False, WorkspaceTypes_e.All)>
            About
        End Enum

#End If
#If _AddPropertyPage_ Then
        Dim m_BoxPage As IXPropertyPage(Of BoxPropertyPage)
        Dim m_BoxData As BoxPropertyPage

#End If
        'function is called when add-in is loading
        Public Overrides Sub OnConnect()
#If (_AddCommandManager_ Or _AddPropertyPage_ Or _AddMacroFeature_) Then
            'creating command manager based on enum
            AddHandler CommandManager.AddCommandGroup(Of Commands_e)().CommandClick, AddressOf OnButtonClick
#If _AddPropertyPage_
            'property page will be created based on the data model and this model will be automatically bound (two-ways)
            m_BoxPage = CreatePage(Of BoxPropertyPage)()
            m_BoxData = New BoxPropertyPage()
            AddHandler m_BoxPage.Closing, AddressOf OnBoxPageClosing
            AddHandler m_BoxPage.Closed, AddressOf OnBoxPageClosed
#End If
#Else
            Application.ShowMessageBox("Hello, __TemplateNamePlaceholderSwAddInVB__! xCAD.NET", MessageBoxIcon_e.Info)
#End If
        End Sub
#If (_AddCommandManager_ Or _AddPropertyPage_ Or _AddMacroFeature_) Then

        'button click handler will pass the enum of the button being clicked
        Private Sub OnButtonClick(ByVal spec As Commands_e)
            Select Case spec
#If (_AddCommandManager_ Or _AddPropertyPage_)
                Case Commands_e.CreateBox
#If _AddPropertyPage_
#Else
                    Dim frontPlane = Application.Documents.Active.Features.OfType(Of IXPlane)().First()
                    CreateBox(frontPlane, 100, 200, 300)
#End If
                    m_BoxPage.Show(m_BoxData)

#End If
#If _AddMacroFeature_
                Case Commands_e.CreateParametricBox
                    Application.Documents.Active.Features.CreateCustomFeature(Of BoxMacroFeatureDefinition, BoxMacroFeatureData, BoxPropertyPage)()

#End If
                Case Commands_e.About
                    Application.ShowMessageBox("About __TemplateNamePlaceholderSwAddInVB__", MessageBoxIcon_e.Info)
            End Select
        End Sub
#End If
#If _AddPropertyPage_ Then
        Private Sub OnBoxPageClosing(reason As PageCloseReasons_e, arg As PageClosingArg)
            If reason = PageCloseReasons_e.Okay Then
                'forbid closing of property page if user has not provided a valid input
                If Not (TypeOf m_BoxData.Location.PlaneOrFace Is IXPlanarRegion) Then
                    arg.Cancel = True
                    arg.ErrorMessage = "Specify plane or face to place box on"
                End If
            End If
        End Sub

        Private Sub OnBoxPageClosed(reason As PageCloseReasons_e)
            If reason = PageCloseReasons_e.Okay Then
                'start box creation process
                CreateBox(CType(m_BoxData.Location.PlaneOrFace, IXPlanarRegion), m_BoxData.Parameters.Width, m_BoxData.Parameters.Height, m_BoxData.Parameters.Length)
            End If
        End Sub
#End If
#If (_AddCommandManager_ Or _AddPropertyPage_) Then

        Private Sub CreateBox(refEnt As IXPlanarRegion, width As Double, height As Double, length As Double)
            Dim doc = Application.Documents.Active

            'freeze current view (optional)
            Using doc.ModelViews.Active.Freeze(True)
                'most of the entities in xCAD.NET are implemented as templates
                'its parameters need to be filled then the entity to be committed to create an element
                Dim sketch = doc.Features.PreCreate2DSketch()
                sketch.ReferenceEntity = refEnt
                Dim rect = sketch.Entities.PreCreateRectangle(New Point(0, 0, 0), width, length, New Vector(1, 0, 0), New Vector(0, 1, 0))
                sketch.Entities.AddRange(rect)
                sketch.Commit()

                sketch.Select(False)

                'it is also possible to access native objects of SOLIDWORKS and use SOLIDWORKS API directly
                Dim extrFeat = doc.Model.FeatureManager.FeatureExtrusion3(True, False, False, swEndConditions_e.swEndCondBlind, swEndConditions_e.swEndCondBlind, height, 0, False, False, False, False, 0, 0, False, False, False, False, True, True, True, swStartConditions_e.swStartSketchPlane, 0, False)

                If extrFeat Is Nothing Then
                    Throw New Exception("Failed to create extrude feature")
                End If
            End Using
        End Sub
#End If
    End Class
End Namespace

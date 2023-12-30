Imports __TemplateNamePlaceholderSwAddInVB__.Sw.My.Resources
Imports System
Imports System.Runtime.InteropServices
Imports Xarial.XCad
Imports Xarial.XCad.Base.Attributes
Imports Xarial.XCad.Documents
Imports Xarial.XCad.Exceptions
Imports Xarial.XCad.Features.CustomFeature
Imports Xarial.XCad.Features.CustomFeature.Delegates
Imports Xarial.XCad.Geometry
Imports Xarial.XCad.Geometry.Structures
Imports Xarial.XCad.SolidWorks
Imports Xarial.XCad.SolidWorks.Documents
Imports Xarial.XCad.SolidWorks.Features.CustomFeature
Imports Xarial.XCad.SolidWorks.Geometry

Namespace __TemplateNamePlaceholderSwAddInVB__.Sw
    Public Class UserException
        Inherits Exception
        Implements IUserException
        Public Sub New(msg As String)
            MyBase.New(msg)
        End Sub
    End Class

    <ComVisible(True)>
    <Guid("871F36E7-B4B7-4026-A341-0A19BBC2BF60")>
    <Icon(GetType(Resources), NameOf(Resources.box_icon))>
    <Title("Box")>'TitleAttribute allows to specify the default (base) name of the feature in the feature manager tree
    Public Class BoxMacroFeatureDefinition
        Inherits SwMacroFeatureDefinition(Of BoxMacroFeatureData, BoxPropertyPage)
        'converting data model from the page to feature data
        'in some cases page and feature data can be of the same class and the conversion is not required
        'this method will be called when user changes the parameters in the property manager page
        Public Overrides Function ConvertPageToParams(app As IXApplication, doc As IXDocument, page As BoxPropertyPage, cudData As BoxMacroFeatureData) As BoxMacroFeatureData
            Return New BoxMacroFeatureData() With {
    .Height = page.Parameters.Height,
    .Length = page.Parameters.Length,
    .Width = page.Parameters.Width,
    .PlaneOrFace = page.Location.PlaneOrFace
}
        End Function

        'converting feature data to the property page
        'this method will be called when existing feature definiton is edited
        Public Overrides Function ConvertParamsToPage(app As IXApplication, doc As IXDocument, par As BoxMacroFeatureData) As BoxPropertyPage
            Dim page = New BoxPropertyPage()
            page.Parameters.Height = par.Height
            page.Parameters.Width = par.Width
            page.Parameters.Length = par.Length
            page.Location.PlaneOrFace = par.PlaneOrFace
            Return page
        End Function

        'this method is called when feature is being inserted and user changes the parameters of the property page (preview purposes)
        'this method will also be called when macro feature is regenerated to create a macro feature body
        'in most cases the procedure of creating the preview body and the generated body is the same
        'but it is also possible to provide custom preview geometry by overriding the CreatePreviewGeometry method
        Public Overrides Function CreateGeometry(app As ISwApplication, doc As ISwDocument, feat As ISwMacroFeature(Of BoxMacroFeatureData), <Out> ByRef alignDim As AlignDimensionDelegate(Of BoxMacroFeatureData)) As ISwBody()
            Dim data = feat.Parameters

            Dim face = data.PlaneOrFace

            Dim pt As Point
            Dim dir As Vector
            Dim refDir As Vector

            If TypeOf face Is IXPlanarRegion Then
                Dim plane = CType(face, IXPlanarRegion).Plane

                pt = plane.Point
                dir = plane.Normal
                refDir = plane.Reference 'it is only possible to create geometry if planar face or plane is selected
            Else
                'it is required to throw the exception which implements the IUserException
                'so this error is displayed to the user
                Throw New UserException("Select planar face or plane for the location")
            End If

            'creating a temp body of the box by providing the center point, direction vectors and size
            Dim box = CType(XGeometryBuilderExtension.CreateSolidBox(app.MemoryGeometryBuilder, pt, dir, refDir, data.Width, data.Length, data.Height).Bodies.First(), ISwBody)

            Dim secondRefDir = refDir.Cross(dir)

            'aligning dimensions. For linear dimensions it is required to specify the origin point and the direction
            'see https://xcad.xarial.com/custom-features/data/dimensions/
            alignDim = Sub(n, d)
                           Select Case n
                               Case NameOf(BoxMacroFeatureData.Width)
                                   AlignLinearDimension(d, pt.Move(CType(refDir * -1, Vector), data.Width / 2).Move(secondRefDir * -1, data.Length / 2), refDir)

                               Case NameOf(BoxMacroFeatureData.Length)
                                   AlignLinearDimension(d, pt.Move(CType(refDir, Vector), data.Width / 2).Move(secondRefDir * -1, data.Length / 2), secondRefDir)

                               Case NameOf(BoxMacroFeatureData.Height)
                                   AlignLinearDimension(d, pt.Move(CType(refDir, Vector), data.Width / 2).Move(secondRefDir * -1, data.Length / 2), dir)
                           End Select
                       End Sub


            Return New ISwBody() {box}
        End Function
    End Class
End Namespace

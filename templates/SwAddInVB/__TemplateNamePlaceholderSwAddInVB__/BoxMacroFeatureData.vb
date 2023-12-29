Imports Xarial.XCad.Features.CustomFeature.Attributes
Imports Xarial.XCad.Features.CustomFeature.Enums
Imports Xarial.XCad.Geometry

Namespace __TemplateNamePlaceholderSwAddInVB__.Sw
    'this corresponds to the data required to generate this macro feature
    Public Class BoxMacroFeatureData
        'face or plane. This entity will be associated as the parent relation and macro feature will update
        'when this entity changes
        Public Property PlaneOrFace As IXEntity

        'marking this parameter to be served as the dimension
        <ParameterDimension(CustomFeatureDimensionType_e.Linear)>
        Public Property Width As Double = 0.1

        <ParameterDimension(CustomFeatureDimensionType_e.Linear)>
        Public Property Height As Double = 0.1

        <ParameterDimension(CustomFeatureDimensionType_e.Linear)>
        Public Property Length As Double = 0.1
    End Class
End Namespace

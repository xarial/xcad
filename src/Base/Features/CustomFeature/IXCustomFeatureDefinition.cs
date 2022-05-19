//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Annotations;
using Xarial.XCad.Documents;
using Xarial.XCad.Features.CustomFeature.Delegates;
using Xarial.XCad.Features.CustomFeature.Enums;
using Xarial.XCad.Features.CustomFeature.Structures;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Features.CustomFeature
{
    /// <summary>
    /// Represents the definition of custom feature where business logic is defined
    /// </summary>
    public interface IXCustomFeatureDefinition
    {
        /// <summary>
        /// Called when the Edit feature menu is clicked from the feature manager tree
        /// </summary>
        /// <param name="app">Pointer to the application</param>
        /// <param name="model">Pointer to the current model where the feature resided</param>
        /// <param name="feature">Pointer to the feature being edited</param>
        /// <returns>Result of the editing</returns>
        /// <remarks>Use this handler to display property manager page or any other user interface to edit feature.
        /// </remarks>
        bool OnEditDefinition(IXApplication app, IXDocument model, IXCustomFeature feature);

        /// <summary>
        /// Called when macro feature is rebuilding
        /// </summary>
        /// <param name="app">Pointer to the SOLIDWORKS application</param>
        /// <param name="model">Pointer to the document where the macro feature being rebuild</param>
        /// <param name="feature">Pointer to the feature</param>
        /// <returns>Result of the operation. Use static methods of <see cref="Structures.CustomFeatureRebuildResult"/>
        /// class to generate results</returns>
        CustomFeatureRebuildResult OnRebuild(IXApplication app, IXDocument model, IXCustomFeature feature);

        /// <summary>
        /// Called when state of the feature is changed (i.e. feature is selected, moved, updated etc.)
        /// Use this method to provide additional dynamic security options on your feature (i.e. do not allow dragging, editing etc.)
        /// </summary>
        /// <param name="app">Pointer to the application</param>
        /// <param name="model">Pointer to the model where the feature resides</param>
        /// <param name="feature">Pointer to the feature to updated state</param>
        /// <returns>State of feature</returns>
        CustomFeatureState_e OnUpdateState(IXApplication app, IXDocument model, IXCustomFeature feature);
    }

    /// <summary>
    /// Represents the custom feature definition bound to the parameters data model
    /// </summary>
    /// <typeparam name="TParams"></typeparam>
    public interface IXCustomFeatureDefinition<TParams> : IXCustomFeatureDefinition
        where TParams : class
    {
        /// <inheritdoc cref="IXCustomFeatureDefinition.OnRebuild(IXApplication, IXDocument, IXCustomFeature)"/>
        /// <param name="parameters">Current parameters of the feature</param>
        /// <param name="alignDim">Handler to align dimensions. Use <see cref="AlignDimension(IXDimension, Point[], Vector, Vector)"/> helper function</param>
        /// <returns>Result of the regeneration</returns>
        CustomFeatureRebuildResult OnRebuild(IXApplication app, IXDocument model, IXCustomFeature feature,
            TParams parameters, out AlignDimensionDelegate<TParams> alignDim);

        /// <summary>
        /// Helper function to align the dimensions of the macro feature
        /// </summary>
        /// <param name="dim">Pointer to the dimension</param>
        /// <param name="pts">Points of the dimension</param>
        /// <param name="dir">Direction of the dimension</param>
        /// <param name="extDir">Dimension extension line</param>
        /// <remarks>Use <see cref="XCustomFeatureDefinitionExtension"/> extension methods for more helper functions to align specific types of dimensions</remarks>
        void AlignDimension(IXDimension dim, Point[] pts, Vector dir, Vector extDir);
    }

    /// <summary>
    /// Represents custom feature with a built-in custom page editor
    /// </summary>
    /// <typeparam name="TParams">Parameters of this custom feature</typeparam>
    /// <typeparam name="TPage">Page editor of this custom feature</typeparam>
    public interface IXCustomFeatureDefinition<TParams, TPage> : IXCustomFeatureDefinition<TParams>
        where TParams : class
        where TPage : class
    {
        /// <summary>
        /// Start insertion of this custom feature
        /// </summary>
        /// <param name="doc">Document where to insert this feature to</param>
        /// <param name="data">Data for insertion</param>
        void Insert(IXDocument doc, TParams data);

        /// <summary>
        /// Called when geometry of this feature needs to be regenerated
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Current document</param>
        /// <param name="data">Data of this custo feature</param>
        /// <param name="alignDim">Function to align dimensions</param>
        /// <returns>Geometry of this macro feature</returns>
        IXBody[] CreateGeometry(IXApplication app, IXDocument doc, TParams data,
            out AlignDimensionDelegate<TParams> alignDim);

        /// <summary>
        /// Creates preview geometry for the custom feature
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="model">Current document</param>
        /// <param name="data">Current data</param>
        /// <param name="page">Current page</param>
        /// <param name="shouldHidePreviewEdit">Delegate for handling if the specific body should be hidden while preview</param>
        /// <param name="assignPreviewColor">Handler to specify the custom color for the preview body</param>
        /// <returns></returns>
        IXBody[] CreatePreviewGeometry(IXApplication app, IXDocument model, TParams data, TPage page,
            out ShouldHidePreviewEditBodyDelegate<TParams, TPage> shouldHidePreviewEdit,
            out AssignPreviewBodyColorDelegate assignPreviewColor);

        /// <summary>
        /// Converter between custom feature parameters and page editor
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Current document</param>
        /// <param name="par">Custom feature parameter</param>
        /// <returns>Corresponding page</returns>
        /// <remarks>This method is called once when editing of the feature is started or new feature is being inserted</remarks>
        TPage ConvertParamsToPage(IXApplication app, IXDocument doc, TParams par);

        /// <summary>
        /// Converts from page to custom feature parameters
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Current document</param>
        /// <param name="page">Page data to convert to custom feature parameters</param>
        /// <param name="curParams">Current parameters</param>
        /// <returns>Corresponding custom feature parameters</returns>
        /// <remarks>This method is called everytime page data is changed</remarks>
        TParams ConvertPageToParams(IXApplication app, IXDocument doc, TPage page, TParams curParams);
    }
}
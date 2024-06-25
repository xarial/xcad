//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Features.CustomFeature.Enums;
using Xarial.XCad.Features.CustomFeature.Structures;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.UI.PropertyPage.Enums;

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
        /// <param name="doc">Pointer to the current model where the feature resided</param>
        /// <param name="feature">Pointer to the feature being edited</param>
        /// <returns>Result of the editing</returns>
        /// <remarks>Use this handler to display property manager page or any other user interface to edit feature.
        /// </remarks>
        bool OnEditDefinition(IXApplication app, IXDocument doc, IXCustomFeature feature);

        /// <summary>
        /// Called when macro feature is rebuilding
        /// </summary>
        /// <param name="app">Pointer to the SOLIDWORKS application</param>
        /// <param name="doc">Pointer to the document where the macro feature being rebuild</param>
        /// <param name="feature">Pointer to the feature</param>
        /// <returns>Result of the operation. Use static methods of <see cref="Structures.CustomFeatureRebuildResult"/>
        /// class to generate results</returns>
        CustomFeatureRebuildResult OnRebuild(IXApplication app, IXDocument doc, IXCustomFeature feature);

        /// <summary>
        /// Called when state of the feature is changed (i.e. feature is selected, moved, updated etc.)
        /// Use this method to provide additional dynamic security options on your feature (i.e. do not allow dragging, editing etc.)
        /// </summary>
        /// <param name="app">Pointer to the application</param>
        /// <param name="doc">Pointer to the model where the feature resides</param>
        /// <param name="feature">Pointer to the feature to updated state</param>
        /// <returns>State of feature</returns>
        CustomFeatureState_e OnUpdateState(IXApplication app, IXDocument doc, IXCustomFeature feature);

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
    /// Represents the custom feature definition bound to the parameters data model
    /// </summary>
    /// <typeparam name="TParams"></typeparam>
    public interface IXCustomFeatureDefinition<TParams> : IXCustomFeatureDefinition
        where TParams : class
    {
        /// <inheritdoc cref="IXCustomFeatureDefinition.OnRebuild(IXApplication, IXDocument, IXCustomFeature)"/>
        /// <returns>Result of the regeneration</returns>
        CustomFeatureRebuildResult OnRebuild(IXApplication app, IXDocument doc, IXCustomFeature<TParams> feature);

        /// <summary>Handler function to align specific dimension</summary>
        /// <param name="feat">Feature to align dimensions for</param>
        /// <param name="paramName">Name of the parameter in the data model which corresponds to this dimension</param>
        /// <param name="dim">Dimension to align</param>
        void OnAlignDimension(IXCustomFeature<TParams> feat, string paramName, IXDimension dim);
    }

    /// <summary>
    /// Custom feature which is managed by property manager page 
    /// </summary>
    public interface IXCustomFeatureEditorDefinition : IXCustomFeatureDefinition
    {
        /// <summary>
        /// Starts insertion of the custom feature
        /// </summary>
        /// <param name="doc">Document</param>
        /// <param name="data">Feature data</param>
        void Insert(IXDocument doc, object data);
    }

    /// <summary>
    /// Represents custom feature with a built-in custom page editor
    /// </summary>
    /// <typeparam name="TParams">Parameters of this custom feature</typeparam>
    /// <typeparam name="TPage">Page editor of this custom feature</typeparam>
    public interface IXCustomFeatureDefinition<TParams, TPage> : IXCustomFeatureEditorDefinition, IXCustomFeatureDefinition<TParams>
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
        /// <param name="feat">Custom feature</param>
        /// <returns>Geometry of this macro feature</returns>
        /// <remarks>Extract current parameters from the feature via <see cref="IXCustomFeature{TParams}.Parameters"/></remarks>
        IXBody[] CreateGeometry(IXApplication app, IXDocument doc, IXCustomFeature<TParams> feat);

        /// <summary>
        /// Creates preview geometry for the custom feature
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="model">Current document</param>
        /// <param name="feat">Current feature</param>
        /// <param name="page">Current page</param>
        /// <returns>Preview bodies</returns>
        /// <remarks>Extract current parameters from the feature via <see cref="IXCustomFeature{TParams}.Parameters"/></remarks>
        IXMemoryBody[] CreatePreviewGeometry(IXApplication app, IXDocument model, IXCustomFeature<TParams> feat, TPage page);

        /// <summary>
        /// Creates property page for the feature
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Current document</param>
        /// <param name="feat">Custom feature. Use <see cref="IXCustomFeature{TParams}.Parameters"/> to read current parameters</param>
        /// <returns>Corresponding page</returns>
        /// <remarks>This method is called once when editing of the feature is started or new feature is being inserted</remarks>
        TPage CreatePropertyPage(IXApplication app, IXDocument doc, IXCustomFeature<TParams> feat);

        /// <summary>
        /// Converts from page to custom feature parameters
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Current document</param>
        /// <param name="page">Page data to convert to custom feature parameters</param>
        /// <param name="curParams">Current parameters</param>
        /// <returns>Corresponding custom feature parameters</returns>
        /// <remarks>This method is called everytime page data is changed</remarks>
        TParams CreateParameters(IXApplication app, IXDocument doc, TPage page, TParams curParams);

        /// <summary>
        /// Handler to assigns the custom color to the preview body
        /// </summary>
        /// <param name="feat">Feature to align dimensions for</param>
        /// <param name="body">Body to assign preview to</param>
        /// <param name="color">Color of the preview body</param>
        void OnAssignPreviewBodyColor(IXCustomFeature<TParams> feat, IXBody body, out System.Drawing.Color color);

        /// <summary>
        /// Handler function to control if the edit body should be hidden during the preview
        /// </summary>
        /// <param name="feat">Feature to align dimensions for</param>
        /// <param name="body">Body which is about to be hidden</param>
        /// <param name="page">Macro feature page</param>
        /// <returns>True to hide body, false to kepe the body visible</returns>
        /// <remarks>usually edit body is hidden during the preview as it is replaced by the macro feature geometry
        /// In some cases user might need to perform multiple selections on edit body and thus hiding it preventing the further selections</remarks>
        bool OnShouldHidePreviewEditBody(IXCustomFeature<TParams> feat, IXBody body, TPage page);

        /// <summary>
        /// Checks if the preview should be updated
        /// </summary>
        /// <param name="feat">Feature to align dimensions for</param>
        /// <param name="oldData">Old parameters</param>
        /// <param name="page">Current page data</param>
        /// <param name="dataChanged">Indicates if the parameters of the data have changed</param>
        /// <remarks>This method is called everytime property manager page data is changed, however this is not always require preview update</remarks>
        bool OnShouldUpdatePreview(IXCustomFeature<TParams> feat, TParams oldData, TPage page, bool dataChanged);

        /// <summary>
        /// Called when the preview of the macro feature updated
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Document</param>
        /// <param name="feat">Feature being edited</param>
        /// <param name="page">Current page data</param>
        /// <remarks>Use <see cref="OnShouldUpdatePreview(IXCustomFeature{TParams}, TParams, TPage, bool)"/> to control if preview needs to be updated</remarks>
        void OnPreviewUpdated(IXApplication app, IXDocument doc, IXCustomFeature<TParams> feat, TPage page);

        /// <summary>
        /// Called when macro feature is being created
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Document</param>
        /// <param name="feat">Feature which is being created (this feature is in not-committed state)</param>
        /// <param name="page">Page data</param>
        /// <remarks>Call <see cref="IXTransaction.Commit(System.Threading.CancellationToken)"/> on the feature to insert it into the tree</remarks>
        void OnFeatureInserting(IXApplication app, IXDocument doc, IXCustomFeature<TParams> feat, TPage page);

        /// <summary>
        /// Called when macro feature is about to be edited before Property Manager Page is opened
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Document</param>
        /// <param name="feat">Feature being edited (null if feature is being inserted)</param>
        /// <param name="page">Page data</param>
        void OnEditingStarted(IXApplication app, IXDocument doc, IXCustomFeature<TParams> feat, TPage page);

        /// <summary>
        /// Called when macro feature is finishing editing and Property Manager Page is about to be closed
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Document</param>
        /// <param name="feat">Feature being edited</param>
        /// <param name="page">Page data</param>
        /// <param name="reason">Closing reason</param>
        void OnEditingCompleting(IXApplication app, IXDocument doc, IXCustomFeature<TParams> feat, TPage page, PageCloseReasons_e reason);

        /// <summary>
        /// Called when macro feature is finished editing and Property Manager Page is closed
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Document</param>
        /// <param name="feat">Feature being edited</param>
        /// <param name="page">Page data</param>
        /// <param name="reason">Closing reason</param>
        /// <remarks>All the changes to the feature are applied or cancelled dependin gon the close reason</remarks>
        void OnEditingCompleted(IXApplication app, IXDocument doc, IXCustomFeature<TParams> feat, TPage page, PageCloseReasons_e reason);
    }
}
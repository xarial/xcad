//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using Xarial.XCad.Base;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Context to draw the custom graphics
    /// </summary>
    public interface IXCustomGraphicsContext : IDisposable, IEnumerable<IXCustomGraphicsRenderer>
    {
        /// <summary>
        /// Registers specific renderer
        /// </summary>
        /// <param name="renderer">Renderer</param>
        void RegisterRenderer(IXCustomGraphicsRenderer renderer);

        /// <summary>
        /// Unregisters renderer
        /// </summary>
        /// <param name="renderer">Renderer</param>
        void UnregisterRenderer(IXCustomGraphicsRenderer renderer);
    }

    /// <summary>
    /// Custom rendered
    /// </summary>
    /// <remarks>Use <see cref="IXCustomGraphicsContext.RegisterRenderer(IXCustomGraphicsRenderer)"/> to register render</remarks>
    public interface IXCustomGraphicsRenderer : IDisposable 
    {
        /// <summary>
        /// Renders the custom graphics
        /// </summary>
        void Render();
    }

    /// <summary>
    /// Represents the model view
    /// </summary>
    public interface IXModelView : IXTransaction
    {
        /// <summary>
        /// Display mode of the view
        /// </summary>
        ViewDisplayMode_e DisplayMode { get; set; }

        /// <summary>
        /// Provides access to custom graphics of this view
        /// </summary>
        IXCustomGraphicsContext CustomGraphicsContext { get; }

        /// <summary>
        /// Freezes all view updates
        /// </summary>
        /// <param name="freeze">True to suppress all updates</param>
        /// <returns>Freeze object, when disposed - view is restored</returns>
        IDisposable Freeze(bool freeze);

        /// <summary>
        /// Transformation of this view related to the model origin
        /// </summary>
        TransformMatrix Transform { get; set; }

        /// <summary>
        /// Transformation of this view related to the screen coordinates
        /// </summary>
        TransformMatrix ScreenTransform { get; }

        /// <summary>
        /// View boundaries
        /// </summary>
        Rectangle ScreenRect { get; }

        /// <summary>
        /// Zooms view to the specified box in XYZ model space
        /// </summary>
        /// <param name="box">Box to zoom to</param>
        void ZoomToBox(Box3D box);

        /// <summary>
        /// Zooms view to fit the model
        /// </summary>
        void ZoomToFit();

        /// <summary>
        /// Zooms to the specified objects
        /// </summary>
        /// <param name="objects">Objects to zoom to</param>
        void ZoomToObjects(IXSelObject[] objects);

        /// <summary>
        /// Refreshes the view
        /// </summary>
        void Update();
    }

    /// <summary>
    /// Represents the view which contains name
    /// </summary>
    public interface IXNamedView : IXModelView 
    {
        /// <summary>
        /// Name of the view
        /// </summary>
        string Name { get; }
    }

    /// <summary>
    /// Represents the one of the standard views
    /// </summary>
    public interface IXStandardView : IXModelView 
    {
        /// <summary>
        /// Type of this standard view
        /// </summary>
        StandardViewType_e Type { get; }
    }

    /// <summary>
    /// Standard 3D views of the model
    /// </summary>
    public enum StandardViewType_e 
    {
        /// <summary>
        /// Front view
        /// </summary>
        Front,

        /// <summary>
        /// Back view
        /// </summary>
        Back,

        /// <summary>
        /// Left view
        /// </summary>
        Left,

        /// <summary>
        /// Right view
        /// </summary>
        Right,

        /// <summary>
        /// Top view
        /// </summary>
        Top,

        /// <summary>
        /// Bottom view
        /// </summary>
        Bottom,

        /// <summary>
        /// Isometric view
        /// </summary>
        Isometric,

        /// <summary>
        /// Trimetric view
        /// </summary>
        Trimetric,

        /// <summary>
        /// Dimetric view
        /// </summary>
        Dimetric
    }
}
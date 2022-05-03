//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Drawing;
using Xarial.XCad.Base;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Context to draw the custom graphics
    /// </summary>
    public interface IXCustomGraphicsContext : IDisposable
    {
    }

    /// <summary>
    /// Represents the model view
    /// </summary>
    public interface IXModelView : IXTransaction
    {
        /// <summary>
        /// Fired when custom graphics can be drawn in the model
        /// </summary>
        event RenderCustomGraphicsDelegate RenderCustomGraphics;

        /// <summary>
        /// Freezes all view updates
        /// </summary>
        /// <param name="freeze">True to suppress all updates</param>
        void Freeze(bool freeze);

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
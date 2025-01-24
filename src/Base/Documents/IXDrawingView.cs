//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents the drawing view on <see cref="IXSheet"/>
    /// </summary>
    public interface IXDrawingView : IXSelObject, IXObjectContainer, IDimensionable, IXTransaction
    {
        /// <summary>
        /// Parent sheet of this drawing view
        /// </summary>
        IXSheet Sheet { get; }

        /// <summary>
        /// Display mode of the view
        /// </summary>
        /// <remarks>null means that display data is inherited from the base view</remarks>
        ViewDisplayMode_e? DisplayMode { get; set; }

        /// <summary>
        /// Bodies scope of this view
        /// </summary>
        IXBody[] Bodies { get; set; }

        /// <summary>
        /// Sketch space of this sheet
        /// </summary>
        IXSketch2D Sketch { get; }

        /// <summary>
        /// Collection of annotations
        /// </summary>
        IXAnnotationRepository Annotations { get; }

        /// <summary>
        /// Visible entities from this view
        /// </summary>
        IXEntityRepository VisibleEntities { get; }

        /// <summary>
        /// Returns the visible polylines of the drawing view
        /// </summary>
        ViewPolylineData[] Polylines { get; }

        /// <summary>
        /// Contains the document referenced by this drawing view
        /// </summary>
        IXDocument3D ReferencedDocument { get; set; }

        /// <summary>
        /// Contains the configuration this drawing view is created from
        /// </summary>
        IXConfiguration ReferencedConfiguration { get; set; }

        /// <summary>
        /// Name of this drawing view
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Location of this drawing view center
        /// </summary>
        Point Location { get; set; }

        /// <summary>
        /// Represents scale of this drawing view
        /// </summary>
        Scale Scale { get; set; }

        /// <summary>
        /// Outline of the view
        /// </summary>
        Rect2D Boundary { get; }

        /// <summary>
        /// Rotation angle of the drawing view
        /// </summary>
        /// <remarks>Value in radians</remarks>
        double Angle { get; set; }

        /// <summary>
        /// View boundary padding
        /// </summary>
        /// <remarks>Padding represents difference between <see cref="Boundary"/> and geometry</remarks>
        Thickness Padding { get; }

        /// <summary>
        /// Transformation of the drawing view in the drawing space relative to the 3D model orientation
        /// </summary>
        TransformMatrix Transformation { get; }

        /// <summary>
        /// Get the base drawing view
        /// </summary>
        /// <remarks>For the root views, base view will be null</remarks>
        IXDrawingView BaseView { get; set; }

        /// <summary>
        /// Gets all views depending on this view
        /// </summary>
        IEnumerable<IXDrawingView> DependentViews { get; }

        /// <summary>
        /// Updates this drawing view
        /// </summary>
        void Update();
    }

    /// <summary>
    /// View created from the <see cref="IXModelView"/>
    /// </summary>
    public interface IXModelViewBasedDrawingView : IXDrawingView 
    {
        /// <summary>
        /// Model view this drawing view is based on
        /// </summary>
        IXModelView SourceModelView { get; set; }
    }

    /// <summary>
    /// View projected from <see cref="IXDrawingView"/>
    /// </summary>
    public interface IXProjectedDrawingView : IXDrawingView
    {
        /// <summary>
        /// Direction of this projection view
        /// </summary>
        ProjectedViewDirection_e Direction { get; set; }
    }

    /// <summary>
    /// Auxiliary drawing view
    /// </summary>
    public interface IXAuxiliaryDrawingView : IXDrawingView
    {
    }

    /// <summary>
    /// Section drawing view
    /// </summary>
    public interface IXSectionDrawingView : IXDrawingView
    {
        /// <summary>
        /// Section of this drawing view
        /// </summary>
        IXSectionLine SectionLine { get; set; }
    }

    /// <summary>
    /// Detailed drawing view
    /// </summary>
    public interface IXDetailedDrawingView : IXDrawingView
    {
        /// <summary>
        /// Circle of this detailed view
        /// </summary>
        IXDetailCircle DetailCircle { get; set; }
    }

    /// <summary>
    /// Flat pattern view
    /// </summary>
    public interface IXFlatPatternDrawingView : IXDrawingView 
    {
        /// <summary>
        /// Sheet metal body of the flat pattern view
        /// </summary>
        IXSolidBody SheetMetalBody { get; set; }

        /// <summary>
        /// Options of flat pattern view
        /// </summary>
        FlatPatternViewOptions_e Options { get; set; }
    }

    /// <summary>
    /// Orientation definition of the <see cref="IXRelativeDrawingView"/>
    /// </summary>
    public class RelativeDrawingViewOrientation 
    {
        /// <summary>
        /// Entity which corresponds to the first orientation
        /// </summary>
        public IXPlanarRegion FirstEntity { get; }

        /// <summary>
        /// Direction of the first entity
        /// </summary>
        public StandardViewType_e FirstDirection { get; }

        /// <summary>
        /// Entity which corresponds to the second orientation
        /// </summary>
        public IXPlanarRegion SecondEntity { get; }

        /// <summary>
        /// Direction of the second entity
        /// </summary>
        public StandardViewType_e SecondDirection { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public RelativeDrawingViewOrientation(IXPlanarRegion firstEntity, StandardViewType_e firstDirection, IXPlanarRegion secondEntity, StandardViewType_e secondDirection)
        {
            FirstEntity = firstEntity;
            FirstDirection = firstDirection;
            SecondEntity = secondEntity;
            SecondDirection = secondDirection;
        }
    }

    /// <summary>
    /// Relative drawing view
    /// </summary>
    public interface IXRelativeDrawingView : IXDrawingView 
    {
        /// <summary>
        /// Orientation of the relative view
        /// </summary>
        RelativeDrawingViewOrientation Orientation { get; set; }
    }
}

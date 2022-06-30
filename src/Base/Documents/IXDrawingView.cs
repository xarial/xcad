//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
        /// View boundary padding
        /// </summary>
        /// <remarks>Padding represents difference between <see cref="Boundary"/> and geometry</remarks>
        Thickness Padding { get; }

        /// <summary>
        /// Get the base drawing view
        /// </summary>
        /// <remarks>For the root views, base view will be null</remarks>
        IXDrawingView BaseView { get; set; }

        /// <summary>
        /// Gets all views depending on this view
        /// </summary>
        IEnumerable<IXDrawingView> DependentViews { get; }
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
        Line SectionLine { get; set; }
    }

    /// <summary>
    /// Detailed drawing view
    /// </summary>
    public interface IXDetailedDrawingView : IXDrawingView
    {
        /// <summary>
        /// Circle of this detailed view
        /// </summary>
        Circle DetailCircle { get; set; }
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
    }
}

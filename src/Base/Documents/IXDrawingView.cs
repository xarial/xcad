//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents the drawing view on <see cref="IXSheet"/>
    /// </summary>
    public interface IXDrawingView : IXSelObject, IXObjectContainer, IXTransaction
    {
        /// <summary>
        /// Name of this drawing view
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Location of this drawing view
        /// </summary>
        Point Location { get; set; }
    }

    /// <summary>
    /// View created from the <see cref="IXModelView"/>
    /// </summary>
    public interface IXModelViewBasedDrawingView : IXDrawingView 
    {
        /// <summary>
        /// Model view this drawing view is based on
        /// </summary>
        IXModelView View { get; set; }
    }
}

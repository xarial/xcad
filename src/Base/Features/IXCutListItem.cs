//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Data;
using Xarial.XCad.Enums;
using Xarial.XCad.Features.Delegates;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Features
{
    /// <summary>
    /// Represents the cut-list item feature
    /// </summary>
    public interface IXCutListItem : IXFeature
    {
        /// <summary>
        /// Status of this cut-list item
        /// </summary>
        CutListStatus_e Status { get; }

        /// <summary>
        /// Type of the cut-list
        /// </summary>
        CutListType_e Type { get; }

        /// <summary>
        /// Collection of properties
        /// </summary>
        IXPropertyRepository Properties { get; }

        /// <summary>
        /// Bodies of this cut-list item
        /// </summary>
        IEnumerable<IXSolidBody> Bodies { get; }

        /// <summary>
        /// Updates cut-lists folder
        /// </summary>
        void Update();
    }

    /// <summary>
    /// Additional methods of <see cref="IXCutListItem"/>
    /// </summary>
    public static class XCutListItemExtension 
    {
        /// <summary>
        /// Gets the quantity of this cut-list-item
        /// </summary>
        /// <param name="item">Input item</param>
        /// <returns>Quantity</returns>
        public static int Quantity(this IXCutListItem item) => item.Bodies.Count();
    }
}

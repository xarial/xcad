//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Features;
using Xarial.XCad.UI;
using Xarial.XCad.Documents.Enums;
using System.Collections.Generic;
using Xarial.XCad.Annotations;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents the configiration (variant) of the document
    /// </summary>
    public interface IXConfiguration : IXObject, IXTransaction, IPropertiesOwner, IDimensionable
    {
        /// <summary>
        /// BOM quantity value
        /// </summary>
        double Quantity { get; }

        /// <summary>
        /// Name of the configuration
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Returns part number of this configuration
        /// </summary>
        string PartNumber { get; }

        /// <summary>
        /// Options for displaying this configuration in BOM
        /// </summary>
        BomChildrenSolving_e BomChildrenSolving { get; }

        /// <summary>
        /// Cut-list items in this configuration (if available)
        /// </summary>
        IEnumerable<IXCutListItem> CutLists { get; }//TODO: create IXPartConfiguration specific to part and move this property there

        /// <summary>
        /// Preview image of this configuration
        /// </summary>
        IXImage Preview { get; }
    }
}
//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
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
    public interface IXConfiguration : IXSelObject, IXTransaction, IDimensionable, IXObjectContainer
    {
        /// <summary>
        /// Id of this configuration
        /// </summary>
        IXIdentifier Id { get; }

        /// <summary>
        /// BOM quantity value
        /// </summary>
        double Quantity { get; }

        /// <summary>
        /// Name of the configuration
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Description of the configuration
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Comment of the configuration
        /// </summary>
        string Comment { get; set; }

        /// <summary>
        /// Returns part number of this configuration
        /// </summary>
        string PartNumber { get; }

        /// <summary>
        /// Collection of properties
        /// </summary>
        IXPropertyRepository Properties { get; }

        /// <summary>
        /// Parent configuration or null if this is a top level configuration
        /// </summary>
        IXConfiguration Parent { get; set; }

        /// <summary>
        /// Options for displaying this configuration in BOM
        /// </summary>
        BomChildrenSolving_e BomChildrenSolving { get; set; }

        /// <summary>
        /// Configuration options
        /// </summary>
        ConfigurationOptions_e Options { get; set; }

        /// <summary>
        /// Preview image of this configuration
        /// </summary>
        IXImage Preview { get; }
    }
}
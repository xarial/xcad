//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
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
    public interface IXConfiguration : IXSelObject, IXTransaction, IPropertiesOwner, IDimensionable
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
        /// Parent configuration or null if this is a top level configuration
        /// </summary>
        IXConfiguration Parent { get; }

        /// <summary>
        /// Options for displaying this configuration in BOM
        /// </summary>
        BomChildrenSolving_e BomChildrenSolving { get; }

        /// <summary>
        /// Preview image of this configuration
        /// </summary>
        IXImage Preview { get; }
    }
}
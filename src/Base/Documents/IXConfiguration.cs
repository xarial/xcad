//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Features;
using Xarial.XCad.UI;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents the configiration (variant) of the document
    /// </summary>
    public interface IXConfiguration : IXObject, IXTransaction, IPropertiesOwner
    {
        /// <summary>
        /// Name of the configuration
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Returns part number of this configuration
        /// </summary>
        string PartNumber { get; }

        /// <summary>
        /// Cut-list items in this configuration (if available)
        /// </summary>
        IXCutListItem[] CutLists { get; }

        /// <summary>
        /// Preview image of this configuration
        /// </summary>
        IXImage Preview { get; }
    }
}
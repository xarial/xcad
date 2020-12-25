//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Features;

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
        /// Cut-list items in this configuration (if available)
        /// </summary>
        IXCutListItem[] CutLists { get; }
    }
}
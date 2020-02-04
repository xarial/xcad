//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.UI.Commands.Enums
{
    /// <summary>
    /// Provides the enumeration of various workspaces
    /// </summary>
    [Flags]
    public enum WorkspaceTypes_e
    {
        /// <summary>
        /// Environment when no documents are loaded
        /// </summary>
        NoDocuments = 1,

        /// <summary>
        /// Part document
        /// </summary>
        Part = 2 << 0,

        /// <summary>
        /// Assembly document
        /// </summary>
        Assembly = 2 << 1,

        /// <summary>
        /// Drawing document
        /// </summary>
        Drawing = 2 << 2,

        /// <summary>
        /// All SOLIDWORKS documents
        /// </summary>
        AllDocuments = Part | Assembly | Drawing,

        /// <summary>
        /// All environments
        /// </summary>
        All = AllDocuments | NoDocuments
    }
}
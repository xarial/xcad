//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents an assembly document (composition of <see cref="IXPart"/> and other <see cref="IXAssembly"/>)
    /// </summary>
    public interface IXAssembly : IXDocument3D
    {
        /// <summary>
        /// Raised when new component is inserted into the assembly
        /// </summary>
        event ComponentInsertedDelegate ComponentInserted;

        /// <summary>
        /// Raised when component is about to be deleted from the assembly
        /// </summary>
        event ComponentDeletingDelegate ComponentDeleting;

        /// <summary>
        /// Raised when component is deleted from the assembly
        /// </summary>
        event ComponentDeletedDelegate ComponentDeleted;

        /// <inheritdoc/>
        new IXAssemblyConfigurationRepository Configurations { get; }

        /// <inheritdoc/>
        new IXAssemblyEvaluation Evaluation { get; }

        /// <summary>
        /// Returns the component which is currently being editied in-context or null
        /// </summary>
        IXComponent EditingComponent { get; }
    }
}
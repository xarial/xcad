//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.Utils.PageBuilder.Base
{
    /// <summary>
    /// Manages binding of <see cref="IPage"/>
    /// </summary>
    public interface IBindingManager
    {
        /// <summary>
        /// List of bindings
        /// </summary>
        IReadOnlyList<IBinding> Bindings { get; }

        /// <summary>
        /// Dependencies manager
        /// </summary>
        IDependencyManager Dependency { get; }

        /// <summary>
        /// Metadata
        /// </summary>
        IMetadata[] Metadata { get; }

        /// <summary>
        /// Loads bindings and dependencies
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="bindings">Bindings</param>
        /// <param name="dependencies">Dependencies</param>
        /// <param name="metadata">Metadata</param>
        void Load(IXApplication app, IReadOnlyList<IBinding> bindings, IRawDependencyGroup dependencies, IMetadata[] metadata);
    }
}
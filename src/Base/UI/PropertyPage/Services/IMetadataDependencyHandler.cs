﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Services
{
    /// <summary>
    /// Handling the dynamic control dependencies
    /// </summary>
    /// <remarks>This is asigned via <see cref="Attributes.DependentOnAttribute"/></remarks>
    public interface IMetadataDependencyHandler
    {
        /// <summary>
        /// Invokes when any of the dependencies controls changed
        /// </summary>
        /// <param name="app">Main application</param>
        /// <param name="source">This control to update state on</param>
        /// <param name="metadata">List of metadata dependencies</param>
        /// <param name="parameter">User parameter</param>
        void UpdateState(IXApplication app, IControl source, IMetadata[] metadata, object parameter);
    }
}

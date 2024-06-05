//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.Utils.PageBuilder.Base
{
    /// <summary>
    /// Handles dependencies
    /// </summary>
    public interface IDependencyManager
    {
        /// <summary>
        /// Initializes this dependency manager
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="depGroup">Raw dependency group</param>
        void Init(IXApplication app, IRawDependencyGroup depGroup);
        
        /// <summary>
        /// Updates all dependencies
        /// </summary>
        void UpdateAll();
    }
}
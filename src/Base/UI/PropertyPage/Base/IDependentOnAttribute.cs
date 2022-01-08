//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Services;

namespace Xarial.XCad.UI.PropertyPage.Base
{
    /// <summary>
    /// Marks the current control to be dependent on other control values
    /// </summary>
    public interface IDependentOnAttribute : IAttribute
    {
        /// <summary>
        /// Handler to resolve dependencies
        /// </summary>
        IDependencyHandler DependencyHandler { get; }

        /// <summary>
        /// List of control tags this control dependent on
        /// </summary>
        object[] Dependencies { get; }
    }
}
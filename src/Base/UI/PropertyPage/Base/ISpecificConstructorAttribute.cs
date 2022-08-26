//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.UI.PropertyPage.Base
{
    /// <summary>
    /// Indicates that the control for this property should be handled with the specific constructor
    /// </summary>
    public interface ISpecificConstructorAttribute : IAttribute
    {
        /// <summary>
        /// Type of specific constructor
        /// </summary>
        Type ConstructorType { get; }
    }
}
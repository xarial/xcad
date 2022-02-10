//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Data
{
    /// <summary>
    /// Specifies that this entity has properties
    /// </summary>
    public interface IPropertiesOwner
    {
        /// <summary>
        /// Collection of properties
        /// </summary>
        IXPropertyRepository Properties { get; }
    }
}

//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;

namespace Xarial.XCad.Data
{
    /// <summary>
    /// Represents the collection of properties
    /// </summary>
    public interface IXPropertyRepository : IXRepository<IXProperty>
    {
        /// <summary>
        /// Pre-creates new property
        /// </summary>
        /// <returns>Property template</returns>
        IXProperty PreCreate();
    }
}

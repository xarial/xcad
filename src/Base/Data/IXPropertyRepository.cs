//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
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
        /// Owner object of the properties
        /// </summary>
        /// <remarks>Value can be one of the following <see cref="Documents.IXDocument"/>, <see cref="Documents.IXConfiguration"/>, <see cref="Features.IXCutListItem"/></remarks>
        IXObject Owner { get; }
    }
}

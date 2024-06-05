﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Indicates that this object can be resilient to the regeneration operations
    /// </summary>
    public interface ISupportsResilience : IXObject
    {
        /// <summary>
        /// Is object resilient to regeneration
        /// </summary>
        bool IsResilient { get; }

        /// <summary>
        /// Converts this object to resilient object
        /// </summary>
        /// <returns>Resilient object</returns>
        IXObject CreateResilient();
    }

    /// <inheritdoc/>
    /// <typeparam name="T">Specific object type</typeparam>
    public interface ISupportsResilience<T> : ISupportsResilience
        where T : IXObject
    {
        /// <summary>
        /// Specific implementation of resilient object
        /// </summary>
        /// <returns>Resilient object</returns>
        new T CreateResilient();
    }
}

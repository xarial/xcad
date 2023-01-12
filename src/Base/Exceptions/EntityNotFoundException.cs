//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Exceptions
{
    /// <summary>
    /// Exception indicates that specific entity is note present within the repository
    /// </summary>
    public class EntityNotFoundException : KeyNotFoundException
    {
        /// <summary>
        /// Defaut constructor
        /// </summary>
        /// <param name="name">Name of the entity</param>
        public EntityNotFoundException(string name) : base($"Entity '{name}' is not found in the repository") 
        {
        }
    }
}

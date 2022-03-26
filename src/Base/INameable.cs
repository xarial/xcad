﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad
{
    /// <summary>
    /// Indicates that object has name
    /// </summary>
    public interface INameable
    {
        /// <summary>
        /// Name of this element
        /// </summary>
        string Name { get; set; }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents the repository of layers
    /// </summary>
    public interface IXLayerRepository : IXRepository<IXLayer>
    {
        /// <summary>
        /// Gets or sets the current layer
        /// </summary>
        IXLayer Active { get; set; }
    }
}
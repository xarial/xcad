//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;

namespace Xarial.XCad
{
    /// <summary>
    /// Indicates that this entity can be placed on the layer
    /// </summary>
    public interface IHasLayer : IXObject
    {
        /// <summary>
        /// Layer of this entity
        /// </summary>
        IXLayer Layer { get; set; }
    }
}

//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Documents.Structures
{
    /// <summary>
    /// Argument of the item deletion event
    /// </summary>
    public class ItemDeleteArgs
    {
        /// <summary>
        /// Specifies if the deleting operation needs to be cancelled
        /// </summary>
        public bool Cancel { get; set; }
    }
}

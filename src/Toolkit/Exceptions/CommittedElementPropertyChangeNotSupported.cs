//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Xarial.XCad.Toolkit.Exceptions
{
    /// <summary>
    /// Exception indicates that proeprty of the <see cref="Base.IXTransaction"/> cannot be modified after the transaction is committed
    /// </summary>
    public class CommittedElementPropertyChangeNotSupported : NotSupportedException
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CommittedElementPropertyChangeNotSupported([CallerMemberName]string prpName = "") 
            : base($"Property '{prpName}' can only be changed when element is not committed") 
        {
        }
    }
}

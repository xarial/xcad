//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Base
{
    /// <summary>
    /// Represents the element which can be precreated
    /// </summary>
    /// <Remarks>Those elements usually got created within the <see cref="IXRepository.AddRange(IEnumerable{TEnt})"/></Remarks>
    public interface IXTransaction
    {
        /// <summary>
        /// Identifies if this element is created or a template
        /// </summary>
        bool IsCommitted { get; }

        void Commit();
    }
}

//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Base;

namespace Xarial.XCad.Documents.Services
{
    /// <summary>
    /// Group of operations (commands)
    /// </summary>
    /// <remarks>This allows to group APi command under as single command for undo-redo purposes</remarks>
    public interface IOperationGroup : IXTransaction, IDisposable
    {
        /// <summary>
        /// Name of the group
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Indicates if this group is temporar and should be restored after the execution
        /// </summary>
        bool IsTemp { get; set; }
    }
}
//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xarial.XCad.Base
{
    /// <summary>
    /// Operation to execute in the <see cref="IXWorkUnit"/>
    /// </summary>
    /// <param name="prg">Progress handler</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public delegate IXWorkUnitResult WorkUnitOperationDelegate(IXProgress prg, CancellationToken cancellationToken);

    /// <summary>
    /// Async operation to execute in the <see cref="IXAsyncWorkUnit"/>
    /// </summary>
    /// <param name="prg">Progress handler</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public delegate Task<IXWorkUnitResult> AsyncWorkUnitOperationDelegate(IXProgress prg, CancellationToken cancellationToken);

    /// <summary>
    /// Result of the work unit
    /// </summary>
    public interface IXWorkUnitResult 
    {
    }

    /// <summary>
    /// Error result
    /// </summary>
    public interface IXWorkUnitErrorResult : IXWorkUnitResult 
    {
        /// <summary>
        /// Error of the work unit
        /// </summary>
        Exception Error { get; }
    }

    /// <summary>
    /// Cancelled result
    /// </summary>
    public interface IXWorkUnitCancelledResult : IXWorkUnitResult
    {
    }

    /// <summary>
    /// User specific result
    /// </summary>
    /// <typeparam name="TRes">Type of result</typeparam>
    public interface IXWorkUnitUserResult<TRes> : IXWorkUnitResult
    {
        /// <summary>
        /// Result
        /// </summary>
        TRes Result { get; }
    }

    /// <summary>
    /// Work unit created by <see cref="Extensions.IXExtension.PreCreateWorkUnit"/>
    /// </summary>
    public interface IXWorkUnit : IXTransaction
    {   
        /// <summary>
        /// RWork unit result
        /// </summary>
        IXWorkUnitResult Result { get; }

        /// <summary>
        /// Operation of this work unit
        /// </summary>
        WorkUnitOperationDelegate Operation { get; set; }
    }

    /// <summary>
    /// Async <see cref="IXWorkUnit"/>
    /// </summary>
    public interface IXAsyncWorkUnit : IXAsyncTransaction 
    {
        /// <summary>
        /// RWork unit result
        /// </summary>
        IXWorkUnitResult Result { get; }

        /// <summary>
        /// Async operation of this work unit
        /// </summary>
        AsyncWorkUnitOperationDelegate AsyncOperation { get; set; }
    }
}

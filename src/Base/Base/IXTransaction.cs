//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xarial.XCad.Base
{
    /// <summary>
    /// Represents the element which can be precreated
    /// </summary>
    /// <Remarks>Those elements usually got created within the <see cref="IXRepository{TEnt}.AddRange(IEnumerable{TEnt}, CancellationToken)"/>
    public interface IXTransaction
    {
        /// <summary>
        /// Identifies if this element is created or a template
        /// </summary>
        bool IsCommitted { get; }

        /// <summary>
        /// Commits this transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        void Commit(CancellationToken cancellationToken);
    }

    /// <summary>
    /// Represents the <see cref="IXTransaction"/> which supports async operation
    /// </summary>
    public interface IXAsyncTransaction
    {/// <summary>
     /// Identifies if this element is created or a template
     /// </summary>
        bool IsCommitted { get; }

        /// <summary>
        /// Commits this transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CommitAsync(CancellationToken cancellationToken);
    }

    /// <summary>
    /// Additional methods for <see cref="IXTransaction"/>
    /// </summary>
    public static class XTransactionExtension
    {
        /// <summary>
        /// Commits the transaction with default cancellation token
        /// </summary>
        /// <param name="transaction">Transaction to commit</param>
        public static void Commit(this IXTransaction transaction) 
            => transaction.Commit(CancellationToken.None);

        /// <summary>
        /// Commits async transaction with the default cancellation token
        /// </summary>
        /// <param name="transaction">Async transaction to commit</param>
        /// <returns>Task</returns>
        public static Task CommitAsync(this IXAsyncTransaction transaction)
            => transaction.CommitAsync(CancellationToken.None);
    }
}

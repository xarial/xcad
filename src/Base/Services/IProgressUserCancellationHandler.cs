//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xarial.XCad.Services
{
    /// <summary>
    /// Service to provide a handler for the user cancellation of the <see cref="IXProgress"/> via ESC button click
    /// </summary>
    public interface IProgressUserCancellationHandler
    {
        /// <summary>
        /// Handle user cancellation
        /// </summary>
        /// <param name="sender">Progress bar</param>
        /// <param name="cts">Cancellation token source passed to <see cref="IXApplication.CreateProgress"/></param>
        void Handle(IXProgress sender, CancellationTokenSource cts);
    }
}
